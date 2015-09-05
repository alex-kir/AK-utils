

#if (UNITY_IPHONE && !UNITY_EDITOR)
#define AKSQLITE_UNITY_IOS_DEVICE
#elif (UNITY_IPHONE && UNITY_EDITOR)
#define AKSQLITE_UNITY_IOS_EDITOR
#elif _PLATFORM_IOS_
#define AKSQLITE_XAMARIN_FORMS_IOS
#elif _PLATFORM_ANDROID_
#define AKSQLITE_XAMARIN_FORMS_ANDROID
#elif WINDOWS_PHONE
#else
//#define AKSQLITE_COMMUNITY_CSHARP_SQLITE
//#define AKSQLITE_WINDOWS_FRAMEWORK_20
#define AKSQLITE_SQLITE_NET
#endif


using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if AKSQLITE_UNITY_IOS_DEVICE || AKSQLITE_XAMARIN_FORMS_IOS || AKSQLITE_XAMARIN_FORMS_ANDROID

using System.Runtime.InteropServices;
using Sqlite_connection = System.IntPtr;
using Sqlite_statement = System.IntPtr;

#elif AKSQLITE_COMMUNITY_CSHARP_SQLITE || AKSQLITE_UNITY_IOS_EDITOR

using Community.CsharpSqlite;
using Sqlite_connection = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite_statement = Community.CsharpSqlite.Sqlite3.Vdbe;

#elif WINDOWS_PHONE

using Sqlite;
using Sqlite_connection = Sqlite.Database;
using Sqlite_statement = Sqlite.Statement;

#elif AKSQLITE_WINDOWS_FRAMEWORK_20

using System.Data.SQLite;

#elif AKSQLITE_SQLITE_NET

using SQLite.Net;

#endif

public partial class AKSqlite : IDisposable
{

#if AKSQLITE_UNITY_IOS_DEVICE

    class Sqlite3
    {
        internal class Wrapper
        {
            [DllImport("__Internal")]
            internal static extern IntPtr sqlite3_column_name(Sqlite_statement statement, int index);

            [DllImport("__Internal")]
            internal static extern IntPtr sqlite3_column_blob(Sqlite_statement statement, int index);

            [DllImport("__Internal")]
            internal static extern IntPtr sqlite3_column_text(Sqlite_statement statement, int index);
        }

        public const int SQLITE_OK = 0;
        public const byte SQLITE_BLOB = 4;
        public const int SQLITE_ROW = 100;

        [DllImport("__Internal")]
        public static extern int sqlite3_open(string filename, out Sqlite_connection sqlite);

        [DllImport("__Internal")]
        public static extern int sqlite3_finalize(Sqlite_statement statement);

        [DllImport("__Internal")]
        public static extern int sqlite3_prepare_v2(Sqlite_connection sqlite, string query, int xz_minus_one, ref Sqlite_statement statement, int xz_zero);

        [DllImport("__Internal")]
        public static extern int sqlite3_step(Sqlite_statement statement);

        public static string sqlite3_column_name(Sqlite_statement statement, int index)
        {
            var ptr = Wrapper.sqlite3_column_name(statement, index);
            return Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport("__Internal")]
        public static extern int sqlite3_column_type(Sqlite_statement statement, int index);

        [DllImport("__Internal")]
        public static extern int sqlite3_column_bytes(Sqlite_statement statement, int index);

        public static byte [] sqlite3_column_blob(Sqlite_statement statement, int index)
        {
            int len = sqlite3_column_bytes(statement, index);
            var ptr = Wrapper.sqlite3_column_blob(statement, index);
            var bytes = new byte[len];
            Marshal.Copy(ptr, bytes, 0, bytes.Length);
            return bytes;
        }


        public static string sqlite3_column_text(Sqlite_statement statement, int index)
        {
            var ptr = Wrapper.sqlite3_column_text(statement, index);
            return Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport("__Internal")]
        public static extern int sqlite3_column_count(Sqlite_statement statement);

        [DllImport("__Internal")]
        public static extern int sqlite3_close(Sqlite_connection sqlite);
    }

#elif AKSQLITE_XAMARIN_FORMS_IOS

    public class Sqlite3
    {
        internal class Wrapper
        {
            [DllImport("sqlite3")]
            internal static extern IntPtr sqlite3_column_name(Sqlite_statement statement, int index);

            [DllImport("sqlite3")]
            internal static extern IntPtr sqlite3_column_blob(Sqlite_statement statement, int index);

            [DllImport("sqlite3")]
            internal static extern IntPtr sqlite3_column_text(Sqlite_statement statement, int index);
        }

        public const int SQLITE_OPEN_READONLY = 0x00000001;
        public const int SQLITE_OK = 0;
        public const byte SQLITE_BLOB = 4;
        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;

        [DllImport("sqlite3")]
        public static extern int sqlite3_open(string filename, out Sqlite_connection sqlite);

        [DllImport("sqlite3")]
        public static extern int sqlite3_open_v2(string filename, out Sqlite_connection sqlite, int flag, IntPtr zVfs);

        [DllImport("sqlite3")]
        public static extern int sqlite3_finalize(Sqlite_statement statement);

        [DllImport("sqlite3")]
        public static extern int sqlite3_prepare_v2(Sqlite_connection sqlite, string query, int xz_minus_one, ref Sqlite_statement statement, int xz_zero);

        [DllImport("sqlite3")]
        public static extern int sqlite3_step(Sqlite_statement statement);

        public static string sqlite3_column_name(Sqlite_statement statement, int index)
        {
            var ptr = Wrapper.sqlite3_column_name(statement, index);
            return Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport("sqlite3")]
        public static extern int sqlite3_column_type(Sqlite_statement statement, int index);

        [DllImport("sqlite3")]
        public static extern int sqlite3_column_bytes(Sqlite_statement statement, int index);

        public static byte [] sqlite3_column_blob(Sqlite_statement statement, int index)
        {
            int len = sqlite3_column_bytes(statement, index);
            var ptr = Wrapper.sqlite3_column_blob(statement, index);
            var bytes = new byte[len];
            Marshal.Copy(ptr, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string sqlite3_column_text(Sqlite_statement statement, int index)
        {
            var ptr = Wrapper.sqlite3_column_text(statement, index);
            return Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport("sqlite3")]
        public static extern int sqlite3_column_count(Sqlite_statement statement);

        [DllImport("sqlite3")]
        public static extern int sqlite3_close(Sqlite_connection sqlite);
    }

#elif AKSQLITE_XAMARIN_FORMS_ANDROID

    public class Sqlite3
    {
    internal class Wrapper
    {
    [DllImport("sqlite3")]
    internal static extern IntPtr sqlite3_column_name(Sqlite_statement statement, int index);

    [DllImport("sqlite3")]
    internal static extern IntPtr sqlite3_column_blob(Sqlite_statement statement, int index);

    [DllImport("sqlite3")]
    internal static extern IntPtr sqlite3_column_text(Sqlite_statement statement, int index);
    }

    public const int SQLITE_OPEN_READONLY = 0x00000001;
    public const int SQLITE_OK = 0;
    public const byte SQLITE_BLOB = 4;
    public const int SQLITE_ROW = 100;
    public const int SQLITE_DONE = 101;

    [DllImport("sqlite3")]
    public static extern int sqlite3_open(string filename, out Sqlite_connection sqlite);

    [DllImport("sqlite3")]
    public static extern int sqlite3_open_v2(string filename, out Sqlite_connection sqlite, int flag, IntPtr zVfs);

    [DllImport("sqlite3")]
    public static extern int sqlite3_finalize(Sqlite_statement statement);

    [DllImport("sqlite3")]
    public static extern int sqlite3_prepare_v2(Sqlite_connection sqlite, string query, int xz_minus_one, ref Sqlite_statement statement, int xz_zero);

    [DllImport("sqlite3")]
    public static extern int sqlite3_step(Sqlite_statement statement);

    public static string sqlite3_column_name(Sqlite_statement statement, int index)
    {
    var ptr = Wrapper.sqlite3_column_name(statement, index);
    return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_type(Sqlite_statement statement, int index);

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_bytes(Sqlite_statement statement, int index);

    public static byte [] sqlite3_column_blob(Sqlite_statement statement, int index)
    {
    int len = sqlite3_column_bytes(statement, index);
    var ptr = Wrapper.sqlite3_column_blob(statement, index);
    var bytes = new byte[len];
    Marshal.Copy(ptr, bytes, 0, bytes.Length);
    return bytes;
    }

    public static string sqlite3_column_text(Sqlite_statement statement, int index)
    {
    var ptr = Wrapper.sqlite3_column_text(statement, index);
    return Marshal.PtrToStringAnsi(ptr);
    }

    [DllImport("sqlite3")]
    public static extern int sqlite3_column_count(Sqlite_statement statement);

    [DllImport("sqlite3")]
    public static extern int sqlite3_close(Sqlite_connection sqlite);
    }

#endif

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
            Dispose();
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

        public void MoveNextAndDispose()
        {
            if (next != null)
                next();
            Dispose();
        }
    }

    public void CreateTable(string table_, string[] keys, string[] fields)
    {
        var ff = string.Join(", ", keys.Concat(fields).Select(it => "`" + it + "`"));
        var kk = string.Join(", ", keys.Select(it => "`" + it + "`"));
        Execute("CREATE TABLE IF NOT EXISTS `" + table_ + "` (" + ff + ", PRIMARY KEY (" + kk + "));").MoveNextAndDispose();
    }

    public void CreateIndex(string table_, params string[] fields)
    {
        var index_name = table_ + "_" + string.Join("_", fields);
        var columns = string.Join(", ", fields.Select(it => "`" + it + "`"));
        Execute(string.Format("CREATE INDEX IF NOT EXISTS `{0}` ON `{1}` ({2});", index_name, table_, columns)).MoveNextAndDispose();
    }

    public void DropTable(string table_)
    {
        Execute("DROP TABLE IF EXISTS`" + table_ + "`").MoveNextAndDispose();
    }

    public void DropIndex(string table_, params string[] fields)
    {
        var index_name = table_ + "_" + string.Join("_", fields);
        Execute(string.Format("DROP INDEX IF EXISTS `{0}`;", index_name)).MoveNextAndDispose();
    }

    public ExecuteResult<Dictionary<string, object>> Read(string table_, string where_ = null, string select_ = null)
    {
        return Execute("SELECT " + (select_ ?? "*") + " FROM `" + table_ + "` WHERE " + (where_ != null ? where_ : " 1=1") + ";");
    }

    public void Write(string table_, IEnumerable<Dictionary<string, object>> rows)
    {
        Execute("BEGIN TRANSACTION;").MoveNextAndDispose();
        foreach (var row in rows)
        {
            var names = new string[row.Count];
            var values = new string[row.Count];
            var parameters = new object[row.Count];
            for (int i = 0; i < row.Count; i++)
            {
                names[i] = "`" + row.ElementAt(i).Key + "`";
                values[i] = "?";
                parameters[i] = row.ElementAt(i).Value;
            }
            Execute("INSERT OR REPLACE INTO `" + table_ + "` (" + string.Join(",", names) + ") VALUES (" + string.Join(",", values) + ");", parameters).MoveNextAndDispose();
        }
        Execute("END TRANSACTION;").MoveNextAndDispose();
    }

    public void BeginTransaction()
    {
        Execute("BEGIN TRANSACTION;").MoveNextAndDispose();
    }

    public void EndTransaction()
    {
        Execute("END TRANSACTION;").MoveNextAndDispose();
    }

    public void Vacuum()
    {
        Execute("VACUUM;").MoveNextAndDispose();
    }

}


#if AKSQLITE_SQLITE_NET

public partial class AKSqlite : IDisposable
{
    private SQLiteConnection sqlite = null;

    public AKSqlite(string filename)
    {
        var connectionString = "Data Source=\"" + filename + "\"";
        sqlite = new SQLiteConnection(null, filename);
    }

    public ExecuteResult<Dictionary<string, object>> Execute(string query, params object[] args)
    {
        var command = sqlite.CreateCommand(query, args);
        var result = command.ExecuteDeferredQuery<Dictionary<string, object>>();

        var enumerator = result.GetEnumerator();

        return new ExecuteResult<Dictionary<string, object>>(() =>
        {
            if (!enumerator.MoveNext())
                return null;
            return enumerator.Current;
        }, () => { enumerator.Dispose(); });
    }

    public void Dispose()
    {
        if (sqlite != null)
        {
            sqlite.Dispose();
            sqlite = null;
        }
    }

}

#elif AKSQLITE_WINDOWS_FRAMEWORK_20

public partial class AKSqlite : IDisposable
{
    private SQLiteConnection sqlite = null;

    public AKSqlite(string filename)
    {
        var connectionString = "Data Source=\"" + filename + "\"";
        sqlite = new SQLiteConnection(connectionString);
        sqlite.Open();
    }

    public ExecuteResult<Dictionary<string, object>> Execute(string query, params object[] args)
    {
        var command = new SQLiteCommand(query, sqlite);

        SQLiteParameter[] parameters = new SQLiteParameter[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            parameters[i] = new SQLiteParameter();
            parameters[i].Value = args[i];
        }
        command.Parameters.AddRange(parameters);

        var reader = command.ExecuteReader();
        int n = reader.FieldCount;
        return new ExecuteResult<Dictionary<string, object>>(() =>
        {
            if (!reader.Read())
                return null;
            return Enumerable.Range(0, n).ToDictionary(it => reader.GetName(it), it => reader.GetValue(it));
        }, () => { reader.Dispose(); });
    }

    public void Dispose()
    {
        if (sqlite != null)
        {
            sqlite.Dispose();
            sqlite = null;
        }
    }

}

#elif WINDOWS_PHONE

partial class AKSqlite : IDisposable
{
    private Sqlite_connection sqlite = default(Sqlite_connection);
    private readonly object _sync = new object();

    public const int SQLITE_OK = 0;
    public const byte SQLITE_BLOB = 4;
    public const int SQLITE_ROW = 100;
    public const int SQLITE_DONE = 101;

    public AKSqlite(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("DatabaseContext.Connect()", filename);
        }

        int errCode = Sqlite3.sqlite3_open(filename, out sqlite);
        if (errCode != SQLITE_OK)
        {
            throw new Exception("Sqlite3.sqlite3_open returns error " + errCode + ", " + Sqlite3ErrorCodeToString(errCode) + ", " + filename);
        }
    }

    private string Sqlite3ErrorCodeToString(int errCode)
    {
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
        if (errCode == SQLITE_ROW) return "SQLITE_ROW";
        if (errCode == SQLITE_DONE) return "SQLITE_DONE";
        return "<Unknown>";
    }

    public ExecuteResult<Dictionary<string, object>> Execute(string query, params object[] args)
    {
        var statement = default(Sqlite_statement);

        int errCode = Sqlite3.sqlite3_prepare_v2(sqlite, query, out statement);
        if (errCode != SQLITE_OK)
        {
            Sqlite3.sqlite3_finalize(statement);
            statement = default(Sqlite_statement);
            throw new Exception("Sqlite3.sqlite3_prepare_v2 returns error " + Sqlite3ErrorCodeToString(errCode) + "; " + query);
        }

        int n = Sqlite3.sqlite3_column_count(statement);



        return new ExecuteResult<Dictionary<string, object>>(() =>
        {
            lock (_sync)
            {
                if (statement == default(Sqlite_statement))
                    return null;
                if (Sqlite3.sqlite3_step(statement) != SQLITE_ROW)
                    return null;

                var row = new Dictionary<string, object>();
                for (int i = 0; i < n; i++)
                {
                    var key = Sqlite3.sqlite3_column_name(statement, i);
                    int type = Sqlite3.sqlite3_column_type(statement, i);
                    if (type == SQLITE_BLOB)
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
        }, () =>
        {
            if (statement != default(Sqlite_statement))
            {
                Sqlite3.sqlite3_finalize(statement);
                statement = default(Sqlite_statement);
            }
        });
    }

    public void Dispose()
    {
        if (sqlite != default(Sqlite_connection))
        {
            Sqlite3.sqlite3_close(sqlite);
            sqlite = default(Sqlite_connection);
        }
    }

}

#else

partial class AKSqlite : IDisposable
{
    private Sqlite_connection sqlite = default(Sqlite_connection);
    private readonly object _sync = new object();


    public AKSqlite(string filename)
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
        var statement = default(Sqlite_statement);

        int errCode = Sqlite3.sqlite3_prepare_v2(sqlite, query, -1, ref statement, 0);
        if (errCode != Sqlite3.SQLITE_OK)
        {
            Sqlite3.sqlite3_finalize(statement);
            statement = default(Sqlite_statement);
            throw new Exception("Sqlite3.sqlite3_prepare_v2 returns error " + Sqlite3ErrorCodeToString(errCode) + "; " + query);
        }

        int n = Sqlite3.sqlite3_column_count(statement);



        return new ExecuteResult<Dictionary<string, object>>(() =>
        {
            lock (_sync)
            {
                if (statement == default(Sqlite_statement))
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
        }, () =>
        {
            if (statement != default(Sqlite_statement))
            {
                Sqlite3.sqlite3_finalize(statement);
                statement = default(Sqlite_statement);
            }
        });
    }

    public void Dispose()
    {
        if (sqlite != default(Sqlite_connection))
        {
            Sqlite3.sqlite3_close(sqlite);
            sqlite = default(Sqlite_connection);
        }
    }

}

#endif
