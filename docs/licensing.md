# Licensing — offline signed keys

DzentoPOS2 licensing is fully **offline**: a shop with no internet can activate a
licence, and the vendor never needs a licence server. Trust comes from an
asymmetric signature, not from a phone-home check.

## 1. Key format

A licence key is a single string:

```
base64url(payloadJson) + "." + base64url(signature)
```

- `payloadJson` — UTF-8 JSON of the payload (see below).
- `signature` — **RSA-SHA256 (PKCS#1 v1.5)** over the exact payload bytes.

Because the byte sequence is signed verbatim, verification never depends on JSON
property ordering.

**Only the vendor holds the private key.** Every customer install embeds the
matching **public** key in code (`DzentoPOS2.App/Services/Licensing/LicensePublicKey.cs`
→ `LicensePublicKey.Pem`); it is deliberately *not* read from `appsettings.json`.
A customer who could swap the public key could sign their own licences, so that
trivial bypass is closed by embedding the key in the assembly.

Only the tuning values remain configurable:

```json
"Licensing": {
  "Enforce": true,
  "GraceDays": 7,
  "ExpiringSoonDays": 14,
  "DefaultPlanName": "Basic"
}
```

### Payload

| Field | Meaning |
|---|---|
| `serial` | Human-readable licence serial / invoice number. |
| `plan` | Plan name, matched against `LicensePlan.Name` (`Basic`, `Standard`, `Professional`, `Enterprise`, …). |
| `deviceId` | Fingerprint of the machine the licence is bound to. |
| `licensedTo` | Optional customer name shown in the UI. |
| `issuedAt` / `expiresAt` | Validity window (UTC). |
| `maxUsers` / `maxBranches` | Ceilings written into the plan on activation. |

> **Key management.** The public key committed in `LicensePublicKey.cs` is the
> vendor's production public key. Its private half is **never** in the repository:
> it lives, password-protected, on the vendor's own machine (see §4). Rotate the
> pair only between releases, because rotating invalidates every previously issued
> key.

## 2. Device fingerprint

`IDeviceIdentifier.GetDeviceId()` returns a stable, hashed machine id:

- **Windows** (`WindowsDeviceIdentifier`): SHA-256 of the registry
  `HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid` (64-bit view). The raw GUID is
  never stored or displayed.
- **Fallback** (`DefaultDeviceIdentifier`): SHA-256 of machine name + OS
  description + architecture, used in tests or when the registry is unreadable.

The fingerprint is shown (and copyable) in **Settings → License → Manage license**
so the vendor can bind a key to the exact machine.

## 3. States and gradual restriction

`LicenseService.GetStatusAsync()` maps the stored licence + device binding + clock
to a state:

| State | Condition | Writes |
|---|---|---|
| `Unlicensed` | no active licence | allowed (trial, `DefaultPlanName` limits) |
| `Active` | bound, more than `ExpiringSoonDays` left | allowed |
| `ExpiringSoon` | bound, ≤ `ExpiringSoonDays` left | allowed (UI warns) |
| `GracePeriod` | past expiry, within `GraceDays` | allowed |
| `Expired` | past expiry + grace | **blocked** (`license.expired` / `license.read_only`) |
| `DeviceMismatch` | licence bound to another machine | allowed only when not enforced |

Limits block **creation at the ceiling** (never corrupt existing data):

- `EnsureCanCreateBranchAsync` → `license.branch_limit`
- `EnsureCanCreateUserAsync` → `license.user_limit`
- `EnsureWritableAsync` → `license.expired` or `license.read_only`

`Enforce = false` turns every guard into a no-op. This is the default in code so
unit/integration tests and development run unconstrained; the shipped
`appsettings.json` sets it to `true`.

### Activating, upgrading and renewing (customer side)

The single active licence is stored in the local database (`License` +
`DeviceActivation` rows), so it survives application updates.

- **Activate (trial → paid):** an administrator opens **Settings → License →
  Manage license**, copies the device fingerprint to the vendor, pastes the signed
  key and clicks Activate. `LicenseService.ActivateAsync` runs
  `Validate` → `UpsertPlanAsync` → `UpsertLicenseAsync` → `BindDeviceAsync`, then
  saves. Activation requires the `IsAdmin` flag.
- **Upgrade a plan / renew:** issue a **new** key (higher plan and/or later
  expiry) and activate it the same way. Activation deactivates every other active
  licence first — a single active licence at a time — updates the plan's
  `MaxUsers`/`MaxBranches` from the new payload, and re-binds the device.
- **Deactivate:** the same dialog's Deactivate button (admin-only) marks the
  current licence inactive and removes this device's activation rows; the app
  falls back to the unlicensed trial.
- **Software update:** the licence lives in `Database/DzentoPOS.db` next to the
  executable, so upgrading the binaries keeps the activation. Only rotating the
  vendor key pair (changing the embedded public key) requires re-issuing keys.

## 4. Vendor tooling — the separate issuer

The signing code and the private key are **not** part of the customer product.
They live in a separate, vendor-only project:

| | Customer app (`DzentoPOS2.*`) | Vendor issuer (`tools/DzentoPOS2.LicenseIssuer`) |
|---|---|---|
| Key | **public only**, embedded in code | **private**, password-protected file |
| Functions | `Verify` / `Validate` only | `GenerateKeyPair` + `Sign` |
| Distribution | shipped to customers | **never shipped** (vendor machine only) |

| | Customer app (`DzentoPOS2.*`) | Vendor issuer (`tools/DzentoPOS2.LicenseIssuer`) |
|---|---|---|
| Key | **public only**, embedded in code | **private**, password-protected file |
| Functions | `Verify` / `Validate` only | `GenerateKeyPair` + `Sign` |
| Distribution | shipped to customers | **never shipped** (vendor machine only) |

Both sides share the cryptographic format through the small `DzentoPOS2.Licensing`
library (payload, validation result, verify/validate codec). It contains no secret
and no signing entry point — only the public key is compiled into the customer app.
Signing and key generation live in `DzentoPOS2.Licensing.Signing`, which only the
issuer and the automated tests reference; the customer app does not.

`DzentoPOS2.LicenseIssuer` is a WinForms app **and** a CLI:

```powershell
# 1) Generate the key pair once. The private key is written as an
#    AES-256 encrypted PKCS#8 PEM; keep it (and its password) secret.
DzentoPOS2.LicenseIssuer.exe --generate --private vendor-private.pem `
    --public vendor-public.pem --password <pwd> [--bits 3072]

# 2) Replace LicensePublicKey.Pem with the new public key and rebuild the app.

# 3) Issue a key bound to a customer's device fingerprint (copy it from the app).
DzentoPOS2.LicenseIssuer.exe --issue --private vendor-private.pem --password <pwd> `
    --plan Standard --device <fingerprint> --days 365 --users 5 --branches 2 --to "Acme"
```

`--issue` prints the key on stdout and the plan/limits on stderr, and appends the
key to `issued-licenses.csv` next to the private key so the vendor can answer
"which key did we issue to whom". Give the key to the customer; they paste it into
**Manage license** and it is verified offline against the embedded public key.

To (re)check a key against a public key (support diagnostics):

```powershell
DzentoPOS2.LicenseIssuer.exe --verify --public vendor-public.pem --key <token> [--device <fingerprint>]
```

It prints `VALID` with the decoded payload, or `INVALID (<code>)` and exits 1.

> **Never ship `tools/DzentoPOS2.LicenseIssuer` or the private key to a customer.**
> The issuer is the only place `Sign` exists and the only place the private key is
> used. Losing it means you can no longer issue licences (you would have to
> generate a new pair and re-issue every customer).

## 5. Enforcement wiring

`ILicenseService` is injected as an **optional** constructor parameter of
`BranchService`, `UserService` and `SaleService`. Production resolves it through
DI; tests that construct those services directly simply omit it. `IBranchLimitPolicy`
is now backed by `LicenseBranchLimitPolicy` (the old always-allow stub is replaced).

## 6. Known limitations

- **No online revocation.** `DeactivateAsync` is local and device-scoped. A key
  that leaked can only be invalidated by issuing a new key pair (rotating the
  embedded public key) or relying on expiry.
- **Fingerprint stability.** `MachineGuid` survives reboots and most updates but a
  re-imaged machine changes it; the customer must then request a re-issued key.
- **Clock trust.** Expiry is evaluated against the local clock (`IBusinessClock`),
  so a user who rewinds the system clock can extend a grace period. This is
  accepted for an offline product; a monotonic high-water-mark could be added later.
