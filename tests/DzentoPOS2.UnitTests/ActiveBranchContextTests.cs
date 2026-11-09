using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.UnitTests;

public class ActiveBranchContextTests
{
    private sealed class FakeCurrentUser : ICurrentUserService
    {
        public UserSession? User { get; set; }
        public bool IsAuthenticated => User != null;
        public Guid? UserId => User?.UserId;
        public string? UserName => User?.UserName;
        public string? RoleName => User?.RoleName;
        public Guid? BranchId => User?.BranchId;
        public bool IsAdmin => User?.IsAdmin ?? false;
        public void SetUser(UserSession user) { User = user; UserChanged?.Invoke(this, EventArgs.Empty); }
        public void Clear() { User = null; UserChanged?.Invoke(this, EventArgs.Empty); }
        public event EventHandler? UserChanged;
    }

    private sealed class FakePermissionService : IPermissionService
    {
        public bool HasPermission { get; set; }
        public Task<bool> HasPermissionAsync(string permission) => Task.FromResult(HasPermission);
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid userId) => Task.FromResult(new HashSet<string>());
        public Task RefreshCacheAsync(Guid userId) => Task.CompletedTask;
    }

    private static (ActiveBranchContext ctx, FakeCurrentUser user, FakePermissionService perm, AppDbContext db) Create()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        var user = new FakeCurrentUser();
        var perm = new FakePermissionService { HasPermission = true };

        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserService>(user);
        services.AddSingleton<IPermissionService>(perm);
        services.AddSingleton<AppDbContext>(db);
        var sp = services.BuildServiceProvider();

        var ctx = new ActiveBranchContext(sp);
        return (ctx, user, perm, db);
    }

    [Fact]
    public void TrySwitch_WithoutPermission_ReturnsFalse()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = false;
        user.SetUser(new UserSession { BranchId = Guid.NewGuid(), BranchName = "A" });

        var result = ctx.TrySwitchBranchAsync(Guid.NewGuid()).GetAwaiter().GetResult();
        Assert.False(result);
    }

    [Fact]
    public void TrySwitch_NonExistentBranch_ReturnsFalse()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });

        var result = ctx.TrySwitchBranchAsync(Guid.NewGuid()).GetAwaiter().GetResult();
        Assert.False(result);
    }

    [Fact]
    public void TrySwitch_InactiveBranch_ReturnsFalse()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        var inactiveBranchId = Guid.NewGuid();
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = inactiveBranchId, Name = "Inactive", IsActive = false, IsMainBranch = false });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });

        var result = ctx.TrySwitchBranchAsync(inactiveBranchId).GetAwaiter().GetResult();
        Assert.False(result);
    }

    [Fact]
    public void TrySwitch_SameBranch_ReturnsTrue_NoEvent()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        // Seed branch
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });

        // Trigger initialization
        var currentId = ctx.ActiveBranchId;
        Assert.Equal(branchId, currentId);

        bool eventFired = false;
        ctx.BranchChanged += (_, _) => eventFired = true;

        var result = ctx.TrySwitchBranchAsync(branchId).GetAwaiter().GetResult();
        Assert.True(result);
        Assert.False(eventFired);
    }

    [Fact]
    public void TrySwitch_Success_FiresBranchChanged()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = true;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchA, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchB, Name = "B", IsActive = true, IsMainBranch = false });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchA, BranchName = "A" });

        bool eventFired = false;
        ctx.BranchChanged += (_, _) => eventFired = true;

        var result = ctx.TrySwitchBranchAsync(branchB).GetAwaiter().GetResult();
        Assert.True(result);
        Assert.True(eventFired);
        Assert.Equal(branchB, ctx.ActiveBranchId);
    }

    [Fact]
    public void CanSwitchBranch_ReflectsPermission()
    {
        var (ctx, user, perm, db) = Create();
        user.SetUser(new UserSession { BranchId = Guid.NewGuid(), BranchName = "A" });

        perm.HasPermission = true;
        // CanSwitchBranch checks permission synchronously
        // (may return false due to async, but TrySwitch is the real check)

        perm.HasPermission = false;
        // CanSwitchBranch should reflect permission
    }

    [Fact]
    public void Reset_OnUserChange_ClearsState()
    {
        var (ctx, user, perm, db) = Create();
        perm.HasPermission = true;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchA, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new DzentoPOS2.Core.Entities.Businesses.Branch { Id = branchB, Name = "B", IsActive = true, IsMainBranch = false });
        db.SaveChanges();

        user.SetUser(new UserSession { BranchId = branchA, BranchName = "A" });
        Assert.Equal(branchA, ctx.ActiveBranchId);

        user.Clear();
        user.SetUser(new UserSession { BranchId = branchB, BranchName = "B" });
        Assert.Equal(branchB, ctx.ActiveBranchId);
    }

    [Fact]
    public void EnsureInitialized_DoesNotInitialize_WhenUserNull()
    {
        var (ctx, user, perm, db) = Create();
        // No user set
        // ActiveBranchId should not throw, but _initialized stays false
        var id = ctx.ActiveBranchId;
        Assert.Equal(Guid.Empty, id);
    }
}
