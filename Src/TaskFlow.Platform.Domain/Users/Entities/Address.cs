using Ardalis.GuardClauses;
using TaskFlow.Platform.Domain.Abstractions;

namespace TaskFlow.Platform.Domain.Users.Entities;

public sealed class Address : Entity
{
    public Address(Guid id, string street, string city, string postalCode, string country)
        : base(id)
    {
        SetStreet(street);
        SetCity(city);
        SetPostalCode(postalCode);
        SetCountry(country);
    }

    private Address()
    {
    }

    public string Street { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public void SetStreet(string street)
    {
        Guard.Against.NullOrWhiteSpace(street, nameof(street));
        var value = street.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(street), 2, 255);
        Street = value;
    }

    public void SetCity(string city)
    {
        Guard.Against.NullOrWhiteSpace(city, nameof(city));
        var value = city.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(city), 2, 100);
        City = value;
    }

    public void SetPostalCode(string postalCode)
    {
        Guard.Against.NullOrWhiteSpace(postalCode, nameof(postalCode));
        var value = postalCode.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(postalCode), 1, 20);
        PostalCode = value;
    }

    public void SetCountry(string country)
    {
        Guard.Against.NullOrWhiteSpace(country, nameof(country));
        var value = country.Trim();
        Guard.Against.OutOfRange(value.Length, nameof(country), 2, 100);
        Country = value;
    }
}
