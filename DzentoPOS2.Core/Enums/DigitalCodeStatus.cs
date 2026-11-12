namespace DzentoPOS2.Core.Enums
{
    /// <summary>
    /// Lifecycle of a digital code (prepaid card / activation key). A code is
    /// imported as <see cref="Available"/>, handed to a customer on a sale
    /// (<see cref="Delivered"/>) and released again when that sale is voided
    /// (<see cref="Available"/>); <see cref="Cancelled"/> removes a code from
    /// circulation without deleting the row.
    /// </summary>
    public enum DigitalCodeStatus
    {
        Available = 0,
        Delivered = 1,
        Cancelled = 2
    }
}
