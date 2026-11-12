namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// The vendor's RSA **public** key, compiled into the customer application.
///
/// <para>
/// It is deliberately embedded in code rather than read from
/// <c>appsettings.json</c>: a customer who could replace the public key could
/// sign their own licences. The matching private key is held only by the vendor
/// (in the separate <c>DzentoPOS2.LicenseIssuer</c> tool) and is never shipped
/// or committed.
/// </para>
///
/// <para>
/// To rotate the key pair, run <c>DzentoPOS2.LicenseIssuer --generate</c>, then
/// replace the value below with the new public key and rebuild the application.
/// Every previously issued key becomes invalid, so rotate only between releases.
/// </para>
/// </summary>
public static class LicensePublicKey
{
    public const string Pem =
        """
        -----BEGIN PUBLIC KEY-----
        MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAtS2lc0cfBQpI+66tp6AQ
        OUMsuw40BoAG1In9ev7UM9QSozsia3due16eX2RprjArQZ7kWZawKE+eZNu5VoFh
        +t+HgFiH2eWE2dsMcHQnvMb6zVJCJCnxIxCsN0rqnq+B3z2qbC1s9Dgw17Yff7d6
        Eo5cnpTCRxjG3Ctkq4usqD245hrS+84wNooVxasn/f8v3FkyvzVW/0qFTYNgrejn
        dGHs8JhSccqAzaggsLF4OYd3K0OLvwr01Q4uiW3DCut9WaiMaMOMQGgGgDk9RnuM
        T4eyervPXiOJKITlp269BY54xfQyCaWBJSttWHssfSoeRCjOonr78eP8kTY5raKC
        HQioN3J1EzO9tEapkVgZWenCWdSyTOtufl80h9pMwtSTMMzmki/NT4rsg/rw9HoW
        +7fQM1JE+P2y/947OjA48JL0S01kacgxd4ITqAAvsknKRDwvo81omXowPVq3hdfz
        DaEdleFKLoqMC+yw3f63MKkPdscCcQnKN355Ayzr/DR5AgMBAAE=
        -----END PUBLIC KEY-----
        """;
}
