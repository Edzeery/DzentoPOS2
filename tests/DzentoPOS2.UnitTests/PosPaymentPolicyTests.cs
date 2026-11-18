using DzentoPOS2.App.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Card payments never reach the cash drawer: they always settle the exact
/// total, so paid == total, remaining == 0 and no customer is required.
/// </summary>
public sealed class PosPaymentPolicyTests
{
    [Fact]
    public void Card_payment_settles_the_exact_total()
    {
        const decimal total = 1234.56m;

        var paid = PosPaymentPolicy.NormalizePaid(PosPaymentMethod.Card, total, 0m);

        Assert.Equal(total, paid);
        Assert.Equal(PosPaymentPolicy.Card, PosPaymentPolicy.ToApiValue(PosPaymentMethod.Card));
        Assert.False(PosPaymentPolicy.RequiresCustomer(PosPaymentMethod.Card, total, paid));
    }

    [Fact]
    public void Cash_payment_keeps_the_typed_value_and_may_leave_a_balance()
    {
        const decimal total = 100m;
        const decimal typed = 40m;

        var paid = PosPaymentPolicy.NormalizePaid(PosPaymentMethod.Cash, total, typed);

        Assert.Equal(typed, paid);
        Assert.True(PosPaymentPolicy.RequiresCustomer(PosPaymentMethod.Cash, total, paid));
    }

    [Fact]
    public void Cash_payment_in_full_does_not_require_a_customer()
    {
        const decimal total = 100m;

        Assert.False(PosPaymentPolicy.RequiresCustomer(PosPaymentMethod.Cash, total, total));
    }
}
