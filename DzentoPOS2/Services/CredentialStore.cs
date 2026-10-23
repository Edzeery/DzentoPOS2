using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace DzentoPOS2.WinForms.Services;

/// <summary>
/// Stores the "remember me" credentials in the Windows Credential Manager
/// (generic credential) instead of a plain text file. <para>
/// The password never leaves the current user profile: it is written with
/// <c>CRED_PERSIST_LOCAL_MACHINE</c> so it survives restarts but stays bound
/// to the Windows account that created it.
/// </para>
/// </summary>
public static class CredentialStore
{
    private const string TargetName = "DzentoPOS2.Login";

    private const int CredTypeGeneric = 1;
    private const int CredPersistLocalMachine = 2;
    private const int ErrorNotFound = 1168;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public IntPtr TargetName;
        public IntPtr Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public IntPtr TargetAlias;
        public IntPtr UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite(ref CREDENTIAL credential, int flags);

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(string target, int type, int reserved, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(string target, int type, int flags);

    [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    private static extern void CredFree(IntPtr buffer);

    /// <summary>True when a credential is stored for the current user.</summary>
    public static bool Exists => TryRead(out _, out _);

    /// <summary>
    /// Reads the stored credential. Returns false when nothing is stored or
    /// when the Windows credential manager is not available.
    /// </summary>
    public static bool TryRead(out string userName, out string password)
    {
        userName = string.Empty;
        password = string.Empty;

        try
        {
            if (!CredRead(TargetName, CredTypeGeneric, 0, out IntPtr pointer))
                return false;

            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(pointer);

                if (credential.UserName != IntPtr.Zero)
                    userName = Marshal.PtrToStringUni(credential.UserName) ?? string.Empty;

                if (credential.CredentialBlob != IntPtr.Zero && credential.CredentialBlobSize > 0)
                {
                    var blob = new byte[credential.CredentialBlobSize];
                    Marshal.Copy(credential.CredentialBlob, blob, 0, credential.CredentialBlobSize);
                    password = Encoding.Unicode.GetString(blob).TrimEnd('\0');
                }

                return true;
            }
            finally
            {
                CredFree(pointer);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Stores (or replaces) the credential of the current user.</summary>
    public static bool Save(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;

        try
        {
            var blob = Encoding.Unicode.GetBytes(password ?? string.Empty);
            var blobPointer = Marshal.AllocCoTaskMem(Math.Max(blob.Length, 1));
            var userPointer = Marshal.StringToCoTaskMemUni(userName);
            var targetPointer = Marshal.StringToCoTaskMemUni(TargetName);

            try
            {
                if (blob.Length > 0)
                    Marshal.Copy(blob, 0, blobPointer, blob.Length);

                var credential = new CREDENTIAL
                {
                    Type = CredTypeGeneric,
                    TargetName = targetPointer,
                    CredentialBlob = blobPointer,
                    CredentialBlobSize = blob.Length,
                    Persist = CredPersistLocalMachine,
                    UserName = userPointer
                };

                return CredWrite(ref credential, 0);
            }
            finally
            {
                Marshal.FreeCoTaskMem(blobPointer);
                Marshal.FreeCoTaskMem(userPointer);
                Marshal.FreeCoTaskMem(targetPointer);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Removes the stored credential (no-op when there is none).</summary>
    public static bool Delete()
    {
        try
        {
            if (CredDelete(TargetName, CredTypeGeneric, 0))
                return true;

            return Marshal.GetLastWin32Error() == ErrorNotFound;
        }
        catch (Win32Exception)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
