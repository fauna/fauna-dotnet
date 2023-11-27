namespace Fauna.Configuration;

/// <summary>
/// The Auth class is responsible for handling authentication, providing functionality to generate bearer tokens.
/// </summary>
public class Auth
{
    private readonly string _secret;

    /// <summary>
    /// Constructs a new Auth instance with the provided secret.
    /// </summary>
    /// <param name="secret">The secret key used for authentication.</param>
    public Auth(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));
        }
        _secret = secret;
    }

    /// <summary>
    /// Generates a bearer token using the secret provided during construction.
    /// </summary>
    /// <returns>A string representing the bearer token.</returns>
    public string Bearer()
    {
        return "Bearer " + _secret;
    }
}
