using Ardalis.GuardClauses;
using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Domain.Auth.Entities;

namespace TaskFlow.Platform.Domain.Users.Entities;

public sealed class User : Entity
{
    public User(Guid id, string firstName, string lastName)
        : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
    }

    private User()
    {
    }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public Guid? AddressId { get; private set; }
    public Address? Address { get; private set; }

    public string? LegalName { get; private set; }

    public string? VatNumber { get; private set; }

    public ApplicationUser? IdentityUser { get; private set; }

    public void SetFirstName(string firstName)
    {
        Guard.Against.NullOrWhiteSpace(firstName, nameof(firstName));
        var value = firstName.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(firstName), 2, 100);
        FirstName = value;
    }

    public void SetLastName(string lastName)
    {
        Guard.Against.NullOrWhiteSpace(lastName, nameof(lastName));
        var value = lastName.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(lastName), 2, 100);
        LastName = value;
    }

    public void SetPhone(string? phone)
    {
        if (phone is null)
        {
            Phone = null;
            return;
        }

        var value = phone.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(phone), 0, 20);
        Phone = value;
    }

    public void SetLegalName(string? legalName)
    {
        if (legalName is null)
        {
            LegalName = null;
            return;
        }

        var value = legalName.Trim();
        if (value.Length == 0)
        {
            LegalName = null;
            return;
        }

        Guard.Against.OutOfRange(value.Length, nameof(legalName), 2, 200);
        LegalName = value;
    }

    public void SetVatNumber(string? vatNumber)
    {
        if (vatNumber is null)
        {
            VatNumber = null;
            return;
        }

        var value = vatNumber.Trim();
        if (value.Length == 0)
        {
            VatNumber = null;
            return;
        }

        Guard.Against.OutOfRange(value.Length, nameof(vatNumber), 2, 50);
        VatNumber = value;
    }

    public void SetAddress(Address? address)
    {
        Address = address;
        AddressId = address?.Id;
    }
}
