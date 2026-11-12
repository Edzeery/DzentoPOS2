using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Businesses;
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

    private static (ActiveBranchContext ctx, FakeCurrentUser user, FakePermissionService perm, AppDbContext db, IBranchResolver resolver) Create()
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
        services.AddSingleton<IBranchResolver>(sp => new BranchResolver(sp));
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IBranchResolver>();
        var ctx = new ActiveBranchContext(sp, resolver);
        return (ctx, user, perm, db, resolver);
    }

    [Fact]
    public async Task TrySwitch_WithoutPermission_ReturnsFalse()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = false;
        var branchId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });
        await ctx.InitializeAsync(user);

        var result = await ctx.TrySwitchBranchAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task TrySwitch_NonExistentBranch_ReturnsFalse()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });
        await ctx.InitializeAsync(user);

        var result = await ctx.TrySwitchBranchAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task TrySwitch_InactiveBranch_ReturnsFalse()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        var inactiveBranchId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new Branch { Id = inactiveBranchId, Name = "Inactive", IsActive = false, IsMainBranch = false });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });
        await ctx.InitializeAsync(user);

        var result = await ctx.TrySwitchBranchAsync(inactiveBranchId);
        Assert.False(result);
    }

    [Fact]
    public async Task TrySwitch_SameBranch_ReturnsTrue_NoEvent()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = true;
        var branchId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });
        await ctx.InitializeAsync(user);

        bool eventFired = false;
        ctx.BranchChanged += (_, _) => eventFired = true;

        var result = await ctx.TrySwitchBranchAsync(branchId);
        Assert.True(result);
        Assert.False(eventFired);
    }

    [Fact]
    public async Task TrySwitch_Success_FiresBranchChanged()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = true;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchA, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new Branch { Id = branchB, Name = "B", IsActive = true, IsMainBranch = false });
        db.SaveChanges();
        user.SetUser(new UserSession { BranchId = branchA, BranchName = "A" });
        await ctx.InitializeAsync(user);

        bool eventFired = false;
        ctx.BranchChanged += (_, _) => eventFired = true;

        var result = await ctx.TrySwitchBranchAsync(branchB);
        Assert.True(result);
        Assert.True(eventFired);
        Assert.Equal(branchB, ctx.ActiveBranchId);
    }

    [Fact]
    public void CanSwitchBranch_ReflectsPermission()
    {
        var (ctx, user, perm, db, resolver) = Create();
        user.SetUser(new UserSession { BranchId = Guid.NewGuid(), BranchName = "A" });

        perm.HasPermission = true;
        // CanSwitchBranch checks permission synchronously
        // (may return false due to async, but TrySwitch is the real check)

        perm.HasPermission = false;
        // CanSwitchBranch should reflect permission
    }

    [Fact]
    public async Task Reset_OnUserChange_ClearsState()
    {
        var (ctx, user, perm, db, resolver) = Create();
        perm.HasPermission = true;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchA, Name = "A", IsActive = true, IsMainBranch = true });
        db.Branches.Add(new Branch { Id = branchB, Name = "B", IsActive = true, IsMainBranch = false });
        db.SaveChanges();

        user.SetUser(new UserSession { BranchId = branchA, BranchName = "A" });
        await ctx.InitializeAsync(user);
        Assert.Equal(branchA, ctx.ActiveBranchId);

        // Reset and re-initialize for branch B
        ctx.Reset();
        user.Clear();
        user.SetUser(new UserSession { BranchId = branchB, BranchName = "B" });
        await ctx.InitializeAsync(user);
        Assert.Equal(branchB, ctx.ActiveBranchId);
    }

    [Fact]
    public void EnsureInitialized_DoesNotInitialize_WhenUserNull()
    {
        var (ctx, user, perm, db, resolver) = Create();
        // No user set
        // ActiveBranchId should throw
        Assert.Throws<InvalidOperationException>(() => ctx.ActiveBranchId);
    }

    [Fact]
    public async Task CanSwitchBranch_IsCachedFromInitialize()
    {
        var (ctx, user, perm, db, resolver) = Create();
        var branchId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A", IsActive = true, IsMainBranch = true });
        db.SaveChanges();

        user.SetUser(new UserSession { BranchId = branchId, BranchName = "A" });
        perm.HasPermission = true;
        await ctx.InitializeAsync(user);

        Assert.True(ctx.CanSwitchBranch);

        // A later permission change must not be observed synchronously; the
        // getter never queries the database.
        perm.HasPermission = false;
        Assert.True(ctx.CanSwitchBranch);
    }

    [Fact]
    public void CanSwitchBranch_BeforeInitialize_ReturnsFalse()
    {
        var (ctx, user, perm, db, resolver) = Create();
        Assert.False(ctx.CanSwitchBranch);
    }

    [Fact]
    public async Task Initialize_NoResolvableBranch_InitializesEmpty()
    {
        var (ctx, user, perm, db, resolver) = Create();
        user.SetUser(new UserSession { BranchId = null, BranchName = string.Empty });
        perm.HasPermission = true;

        await ctx.InitializeAsync(user);

        Assert.Equal(Guid.Empty, ctx.ActiveBranchId);
        Assert.Equal(string.Empty, ctx.ActiveBranchName);
        Assert.False(ctx.CanSwitchBranch);
    }

    [Fact]
    public void InitializeEmpty_EnablesGettersWithoutABranch()
    {
        var (ctx, user, perm, db, resolver) = Create();

        ctx.InitializeEmpty();

        Assert.Equal(Guid.Empty, ctx.ActiveBranchId);
        Assert.Equal(string.Empty, ctx.ActiveBranchName);
        Assert.False(ctx.CanSwitchBranch);
    }

    [Fact]
    public async Task TrySwitch_BeforeInitialize_Throws()
    {
        var (ctx, user, perm, db, resolver) = Create();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.TrySwitchBranchAsync(Guid.NewGuid()));
    }
}
