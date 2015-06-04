//
//  DatabaseContext.cs
//
//  Utilities and wrappers: https://github.com/alex-kir/AK-utils
//
//  Copyright (c) 2015 - Alexander Kirienko
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//

#if (UNITY_IPHONE && !UNITY_EDITOR)
#define UNITY_IOS_DEVICE
#elif (UNITY_IPHONE && UNITY_EDITOR)
#define UNITY_IOS_EDITOR
#elif _PLATFORM_IOS_
#define XAMARIN_FORMS_IOS
#elif _PLATFORM_ANDROID_
#define XAMARIN_FORMS_ANDROID
#elif false
#define COMMUNITY_CSHARP_SQLITE
#endif

using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if UNITY_IOS_DEVICE

using System.Runtime.InteropServices;

using Sqlite_sqlite3 = System.IntPtr;
using Sqlite_Vdbe = System.IntPtr;

class Sqlite3
{
    internal class Wrapper
    {
        [DllImport("__Internal")]
        internal static extern IntPtr sqlite3_column_name(Sqlite_Vdbe statement, int index);

        [DllImport("__Internal")]
        internal static extern IntPtr sqlite3_column_blob(Sqlite_Vdbe statement, int index);

        [DllImport("__Internal")]
        internal static extern IntPtr sqlite3_column_text(Sqlite_Vdbe statement, int index);
    }

    public const int SQLITE_OK = 0;
    public const byte SQLITE_BLOB = 4;
    public const int SQLITE_ROW = 100;

    [DllImport("__Internal")]
    public static extern int sqlite3_open(string filename, out Sqlite_sqlite3 sqlite);

    [DllImport("__Internal")]
    public static extern int sqlite3_finalize(Sqlite_Vdbe statement);

    [DllImport("__Internal")]
    public static extern int sqlite3_prepare_v2(Sqlite_sqlite3 sqlite, string query, int xz_minus_one, ref Sqlite_Vdbe statement, int xz_zero);

    [DllImport("__Internal")]
    public static extern int sqlite3_step(Sqlite_Vdbe statement);

    public static string sqlite3_column_name(Sqlite_Vdbe statement, int index)
    {
        var ptr = Wrapper.sqlite3_column_name(statement, index);
        return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("__Internal")]
    public static extern int sqlite3_column_type(Sqlite_Vdbe statement, int index);

    [DllImport("__Internal")]
    public static extern int sqlite3_column_bytes(Sqlite_Vdbe statement, int index);

    public static byte [] sqlite3_column_blob(Sqlite_Vdbe statement, int index)
    {
        int len = sqlite3_column_bytes(statement, index);
        var ptr = Wrapper.sqlite3_column_blob(statement, index);
        var bytes = new byte[len];
        Marshal.Copy(ptr, bytes, 0, bytes.Length);
        return bytes;
    }


    public static string sqlite3_column_text(Sqlite_Vdbe statement, int index)
    {
        var ptr = Wrapper.sqlite3_column_text(statement, index);
        return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("__Internal")]
    public static extern int sqlite3_column_count(Sqlite_Vdbe statement);

    [DllImport("__Internal")]
    public static extern int sqlite3_close(Sqlite_sqlite3 sqlite);
}

#elif XAMARIN_FORMS_IOS

using System.Runtime.InteropServices;

using Sqlite_sqlite3 = System.IntPtr;
using Sqlite_Vdbe = System.IntPtr;

public class Sqlite3
{
    internal class Wrapper
    {
        [DllImport("sqlite3")]
        internal static extern IntPtr sqlite3_column_name(Sqlite_Vdbe statement, int index);

        [DllImport("sqlite3")]
        internal static extern IntPtr sqlite3_column_blob(Sqlite_Vdbe statement, int index);

        [DllImport("sqlite3")]
        internal static extern IntPtr sqlite3_column_text(Sqlite_Vdbe statement, int index);
    }

    public const int SQLITE_OPEN_READONLY = 0x00000001;
    public const int SQLITE_OK = 0;
    public const byte SQLITE_BLOB = 4;
    public const int SQLITE_ROW = 100;
    public const int SQLITE_DONE = 101;

    [DllImport("sqlite3")]
    public static extern int sqlite3_open(string filename, out Sqlite_sqlite3 sqlite);

    [DllImport("sqlite3")]
    public static extern int sqlite3_open_v2(string filename, out Sqlite_sqlite3 sqlite, int flag, IntPtr zVfs);

    [DllImport("sqlite3")]
    public static extern int sqlite3_finalize(Sqlite_Vdbe statement);

    [DllImport("sqlite3")]
    public static extern int sqlite3_prepare_v2(Sqlite_sqlite3 sqlite, string query, int xz_minus_one, ref Sqlite_Vdbe statement, int xz_zero);

    [DllImport("sqlite3")]
    public static extern int sqlite3_step(Sqlite_Vdbe statement);

    public static string sqlite3_column_name(Sqlite_Vdbe statement, int index)
    {
        var ptr = Wrapper.sqlite3_column_name(statement, index);
        return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_type(Sqlite_Vdbe statement, int index);

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_bytes(Sqlite_Vdbe statement, int index);

    public static byte [] sqlite3_column_blob(Sqlite_Vdbe statement, int index)
    {
        int len = sqlite3_column_bytes(statement, index);
        var ptr = Wrapper.sqlite3_column_blob(statement, index);
        var bytes = new byte[len];
        Marshal.Copy(ptr, bytes, 0, bytes.Length);
        return bytes;
    }

    public static string sqlite3_column_text(Sqlite_Vdbe statement, int index)
    {
        var ptr = Wrapper.sqlite3_column_text(statement, index);
        return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_count(Sqlite_Vdbe statement);

    [DllImport("sqlite3")]
    public static extern int sqlite3_close(Sqlite_sqlite3 sqlite);
}

#elif XAMARIN_FORMS_ANDROID

using System.Runtime.InteropServices;

using Sqlite_sqlite3 = System.IntPtr;
using Sqlite_Vdbe = System.IntPtr;

public class Sqlite3
{
internal class Wrapper
{
[DllImport("sqlite3")]
internal static extern IntPtr sqlite3_column_name(Sqlite_Vdbe statement, int index);

[DllImport("sqlite3")]
internal static extern IntPtr sqlite3_column_blob(Sqlite_Vdbe statement, int index);

[DllImport("sqlite3")]
internal static extern IntPtr sqlite3_column_text(Sqlite_Vdbe statement, int index);
}

public const int SQLITE_OPEN_READONLY = 0x00000001;
public const int SQLITE_OK = 0;
public const byte SQLITE_BLOB = 4;
public const int SQLITE_ROW = 100;
public const int SQLITE_DONE = 101;

[DllImport("sqlite3")]
public static extern int sqlite3_open(string filename, out Sqlite_sqlite3 sqlite);

[DllImport("sqlite3")]
public static extern int sqlite3_open_v2(string filename, out Sqlite_sqlite3 sqlite, int flag, IntPtr zVfs);

[DllImport("sqlite3")]
public static extern int sqlite3_finalize(Sqlite_Vdbe statement);

[DllImport("sqlite3")]
public static extern int sqlite3_prepare_v2(Sqlite_sqlite3 sqlite, string query, int xz_minus_one, ref Sqlite_Vdbe statement, int xz_zero);

[DllImport("sqlite3")]
public static extern int sqlite3_step(Sqlite_Vdbe statement);

public static string sqlite3_column_name(Sqlite_Vdbe statement, int index)
{
var ptr = Wrapper.sqlite3_column_name(statement, index);
return Marshal.PtrToStringAnsi(ptr);
}

[DllImport("sqlite3")]
public static extern int sqlite3_column_type(Sqlite_Vdbe statement, int index);

[DllImport("sqlite3")]
public static extern int sqlite3_column_bytes(Sqlite_Vdbe statement, int index);

public static byte [] sqlite3_column_blob(Sqlite_Vdbe statement, int index)
{
int len = sqlite3_column_bytes(statement, index);
var ptr = Wrapper.sqlite3_column_blob(statement, index);
var bytes = new byte[len];
Marshal.Copy(ptr, bytes, 0, bytes.Length);
return bytes;
}

public static string sqlite3_column_text(Sqlite_Vdbe statement, int index)
{
var ptr = Wrapper.sqlite3_column_text(statement, index);
return Marshal.PtrToStringAnsi(ptr);
}

[DllImport("sqlite3")]
public static extern int sqlite3_column_count(Sqlite_Vdbe statement);

[DllImport("sqlite3")]
public static extern int sqlite3_close(Sqlite_sqlite3 sqlite);
}

#elif COMMUNITY_CSHARP_SQLITE

using Community.CsharpSqlite;

using Sqlite_sqlite3 = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite_Vdbe = Community.CsharpSqlite.Sqlite3.Vdbe;

#endif


public sealed class DatabaseContext : IDisposable
{
    public sealed class ExecuteResult<T> : IEnumerable<T>, IDisposable
    {
        Func<T> next;
        Action dispose;
        internal ExecuteResult(Func<T> next, Action dispose)
        {
            this.next = next;
            this.dispose = dispose;
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (next != null)
            {
                var ret = next();
                if (ret == null)
                    break;
                yield return ret;
            }
            if (dispose != null)
                dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public void Dispose()
        {
            if (dispose != null)
                dispose();
            dispose = null;
            next = null;
        }
    }

    private Sqlite_sqlite3 sqlite = default(Sqlite_sqlite3);
    private readonly object _sync = new object();

    public DatabaseContext(string filename = null)
    {
        if (filename != null)
            Connect(filename);
    }

    private void Connect(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("DatabaseContext.Connect()", filename);
        }
        int errCode = Sqlite3.sqlite3_open(filename, out sqlite);
        if (errCode != Sqlite3.SQLITE_OK)
        {
            throw new Exception("Sqlite3.sqlite3_open returns error " + errCode + ", " + Sqlite3ErrorCodeToString(errCode) + ", " + filename);
        }
    }

    private string Sqlite3ErrorCodeToString(int errCode)
    {
#if COMMUNITY_CSHARP_SQLITE
        if (errCode == Sqlite3.SQLITE_ERROR) return "SQLITE_ERROR";
        if (errCode == Sqlite3.SQLITE_INTERNAL) return "SQLITE_INTERNAL";
        if (errCode == Sqlite3.SQLITE_PERM) return "SQLITE_PERM";
        if (errCode == Sqlite3.SQLITE_ABORT) return "SQLITE_ABORT";
        if (errCode == Sqlite3.SQLITE_BUSY) return "SQLITE_BUSY";
        if (errCode == Sqlite3.SQLITE_LOCKED) return "SQLITE_LOCKED";
        if (errCode == Sqlite3.SQLITE_NOMEM) return "SQLITE_NOMEM";
        if (errCode == Sqlite3.SQLITE_READONLY) return "SQLITE_READONLY";
        if (errCode == Sqlite3.SQLITE_INTERRUPT) return "SQLITE_INTERRUPT";
        if (errCode == Sqlite3.SQLITE_IOERR) return "SQLITE_IOERR";
        if (errCode == Sqlite3.SQLITE_CORRUPT) return "SQLITE_CORRUPT";
        if (errCode == Sqlite3.SQLITE_NOTFOUND) return "SQLITE_NOTFOUND";
        if (errCode == Sqlite3.SQLITE_FULL) return "SQLITE_FULL";
        if (errCode == Sqlite3.SQLITE_CANTOPEN) return "SQLITE_CANTOPEN";
        if (errCode == Sqlite3.SQLITE_PROTOCOL) return "SQLITE_PROTOCOL";
        if (errCode == Sqlite3.SQLITE_EMPTY) return "SQLITE_EMPTY";
        if (errCode == Sqlite3.SQLITE_SCHEMA) return "SQLITE_SCHEMA";
        if (errCode == Sqlite3.SQLITE_TOOBIG) return "SQLITE_TOOBIG";
        if (errCode == Sqlite3.SQLITE_CONSTRAINT) return "SQLITE_CONSTRAINT";
        if (errCode == Sqlite3.SQLITE_MISMATCH) return "SQLITE_MISMATCH";
        if (errCode == Sqlite3.SQLITE_MISUSE) return "SQLITE_MISUSE";
        if (errCode == Sqlite3.SQLITE_NOLFS) return "SQLITE_NOLFS";
        if (errCode == Sqlite3.SQLITE_AUTH) return "SQLITE_AUTH";
        if (errCode == Sqlite3.SQLITE_FORMAT) return "SQLITE_FORMAT";
        if (errCode == Sqlite3.SQLITE_RANGE) return "SQLITE_RANGE";
        if (errCode == Sqlite3.SQLITE_NOTADB) return "SQLITE_NOTADB";
        if (errCode == Sqlite3.SQLITE_ROW) return "SQLITE_ROW";
        if (errCode == Sqlite3.SQLITE_DONE) return "SQLITE_DONE";
#else
        if (errCode == 1) return "SQLITE_ERROR";
        if (errCode == 2) return "SQLITE_INTERNAL";
        if (errCode == 3) return "SQLITE_PERM";
        if (errCode == 4) return "SQLITE_ABORT";
        if (errCode == 5) return "SQLITE_BUSY";
        if (errCode == 6) return "SQLITE_LOCKED";
        if (errCode == 7) return "SQLITE_NOMEM";
        if (errCode == 8) return "SQLITE_READONLY";
        if (errCode == 9) return "SQLITE_INTERRUPT";
        if (errCode == 10) return "SQLITE_IOERR";
        if (errCode == 11) return "SQLITE_CORRUPT";
        if (errCode == 12) return "SQLITE_NOTFOUND";
        if (errCode == 13) return "SQLITE_FULL";
        if (errCode == 14) return "SQLITE_CANTOPEN";
        if (errCode == 15) return "SQLITE_PROTOCOL";
        if (errCode == 16) return "SQLITE_EMPTY";
        if (errCode == 17) return "SQLITE_SCHEMA";
        if (errCode == 18) return "SQLITE_TOOBIG";
        if (errCode == 19) return "SQLITE_CONSTRAINT";
        if (errCode == 20) return "SQLITE_MISMATCH";
        if (errCode == 21) return "SQLITE_MISUSE";
        if (errCode == 22) return "SQLITE_NOLFS";
        if (errCode == 23) return "SQLITE_AUTH";
        if (errCode == 24) return "SQLITE_FORMAT";
        if (errCode == 25) return "SQLITE_RANGE";
        if (errCode == 26) return "SQLITE_NOTADB";
        if (errCode == 27) return "SQLITE_NOTICE";
        if (errCode == 28) return "SQLITE_WARNING";

        if (errCode == Sqlite3.SQLITE_ROW) return "SQLITE_ROW";
        if (errCode == Sqlite3.SQLITE_DONE) return "SQLITE_DONE";
#endif
        return "<Unknown>";
    }

    public ExecuteResult<Dictionary<string, object>> Execute(string query, params object[] args)
    {
        var statement = default(Sqlite_Vdbe);

        int errCode = Sqlite3.sqlite3_prepare_v2(sqlite, query, -1, ref statement, 0);
        if (errCode != Sqlite3.SQLITE_OK)
        {
            Sqlite3.sqlite3_finalize(statement);
            statement = default(Sqlite_Vdbe);
            throw new Exception("Sqlite3.sqlite3_prepare_v2 returns error " + Sqlite3ErrorCodeToString(errCode) + "; " + query);
        }

        int n = Sqlite3.sqlite3_column_count(statement);



        return new ExecuteResult<Dictionary<string, object>>(() => {
            lock (_sync)
            {
                if (statement == default(Sqlite_Vdbe))
                    return null;
                if (Sqlite3.sqlite3_step(statement) != Sqlite3.SQLITE_ROW)
                    return null;

                var row = new Dictionary<string, object>();
                for (int i = 0; i < n; i++)
                {
                    var key = Sqlite3.sqlite3_column_name(statement, i);
                    int type = Sqlite3.sqlite3_column_type(statement, i);
                    if (type == Sqlite3.SQLITE_BLOB)
                    {
                        row[key] = Sqlite3.sqlite3_column_blob(statement, i);
                    }
                    else
                    {
                        row[key] = Sqlite3.sqlite3_column_text(statement, i);
                    }
                }
                return row;
            }
        }, () => {
            if (statement != default(Sqlite_Vdbe))
            {
                Sqlite3.sqlite3_finalize(statement);
                statement = default(Sqlite_Vdbe);
            }
        });
    }

    public void Dispose()
    {
        if (sqlite != default(Sqlite_sqlite3))
        {
            Sqlite3.sqlite3_close(sqlite);
            sqlite = default(Sqlite_sqlite3);
        }
    }
}
