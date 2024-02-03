using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace shookayNET
{
    enum EncodingType
    {
        UTF8,
        UTF16,
        UTF32
    };

    enum WordMatchMethod
    {
        Exact,
        Within
    };

    internal static partial  class ExternalMethods
    {
        [LibraryImport("shookay")]     
        public static partial IntPtr CreateSearchEngine();

        [LibraryImport("shookay")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool RemoveEntry(IntPtr engine, int id);
        [LibraryImport("shookay")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool RefreshEntryUTF16(IntPtr engine, int id, IntPtr wyrazenie);

        [LibraryImport("shookay")]
        public static partial IntPtr FindUTF16 (IntPtr engine, IntPtr wyrazenie, out int length, WordMatchMethod method);

        [LibraryImport("shookay")]
        public static partial IntPtr AddEntryUTF16(IntPtr engine,int id, IntPtr wyrazenie);

        [LibraryImport("shookay")]
        public static partial void DeliverEntriesWithCallback(IntPtr engine, IntPtr dataPointer, EncodingType encodingType, Commons.ProgressCallback progressCallback);        

        [LibraryImport("shookay")]
        public static partial void DeliverEntries(IntPtr engine, IntPtr dataPointer,  EncodingType encodingType);

    }

    public static class Commons
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProgressCallback(float progress);
    }


    public class ShookayWrapper<T> where T : class
    {
        private  ObjectToEntry _delegateMethod;
        private readonly List<T> _objects;
        private readonly IntPtr _searchEngine;
        public delegate KeyValuePair<int, string> ObjectToEntry(T obj);
        private readonly string _idColumnName;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProgressCallback(int progress);

        public ShookayWrapper(List<T> objects, ObjectToEntry? delegateMethod=null,string? idColumnName=null)            
        {
            _objects = objects;
            _delegateMethod = delegateMethod;
            _idColumnName = idColumnName;
            _searchEngine = ExternalMethods.CreateSearchEngine();
        }

        

       
        internal KeyValuePair<int, string> InternalExtractor(T obj)
        {
            return Extractor.WordExtractor(obj,_idColumnName);
        }

     

        public async Task PrepareEntries()
        {
            _delegateMethod ??= InternalExtractor;

            await Task.Run(() =>
            {

                var entries = new Dictionary<int, string>();
                foreach (var item in _objects)
                {
                    var ret = _delegateMethod(item);
                    entries.Add(ret.Key, ret.Value);
                }


                byte[] dane = EncodeDictionaryToBinary(entries);
                GCHandle gch = GCHandle.Alloc(dane, GCHandleType.Pinned);
                try
                {
                    ExternalMethods.DeliverEntries(_searchEngine, gch.AddrOfPinnedObject(),  EncodingType.UTF16);
                }
                finally
                {
                    gch.Free();
                }
            });
        }

     
        public async Task DeliverEntriesReportProgress(Commons.ProgressCallback progressCallback)
        {
            _delegateMethod ??= InternalExtractor;

            await Task.Run(() =>
            {
                var entries = new Dictionary<int, string>();
                foreach (var item in _objects)
                {
                    var ret = _delegateMethod(item);
                    entries.Add(ret.Key, ret.Value);
                }
                byte[] dane = EncodeDictionaryToBinary(entries);
                GCHandle gch = GCHandle.Alloc(dane, GCHandleType.Pinned);
                try
                {
                  
                    ExternalMethods.DeliverEntriesWithCallback(_searchEngine, gch.AddrOfPinnedObject(), EncodingType.UTF16 ,progressCallback);
                }
                finally
                {
                    gch.Free();
                }
            });
        }

        
        private static byte[] EncodeDictionaryToBinary(Dictionary<int, string> map)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream, Encoding.Unicode);            
                // Add number of entries
                int numEntries = map.Count;
                writer.Write(numEntries);
                foreach (var pair in map)
                {
                    // Add key
                    writer.Write(pair.Key);
                    // Add the length of the string
                    string str = pair.Value;
                    int strLength = str.Length * sizeof(char); // char in C# is UTF-16
                    writer.Write(strLength);
                    // Add string
                    writer.Write(str.ToCharArray());
                }            
            return memoryStream.ToArray();
        }

        public async Task<bool> RemoveEntry(int id)
        {
            bool result=false;
            await Task.Run(() =>
            {         
                 result = ExternalMethods.RemoveEntry(_searchEngine, id);
            });
            return result;
        }

        public async Task<bool> RefreshEntry(int id, string entry)
        {
            bool result = false;
            await Task.Run(() =>
            {

                var utf16Bytes = Encoding.Unicode.GetBytes(entry + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    result = ExternalMethods.RefreshEntryUTF16(_searchEngine, id, gch.AddrOfPinnedObject());
                }
                finally
                {
                    gch.Free();
                }
            });
            return result;
        }

        public async Task<bool> RefreshEntry(T obj)
        {
            bool result = false;
            await Task.Run(() =>
            {
                var entry = _delegateMethod(obj);
                var utf16Bytes = Encoding.Unicode.GetBytes(entry + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    result = ExternalMethods.RefreshEntryUTF16(_searchEngine, id, gch.AddrOfPinnedObject());
                }
                finally
                {
                    gch.Free();
                }
            });
            return result;
        }


        public async Task AddEntry(int id, string entry)
        {
            await Task.Run(() =>
            {
           
                var utf16Bytes = Encoding.Unicode.GetBytes(entry + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    resultsPtr = ExternalMethods.AddEntryUTF16(_searchEngine,id, gch.AddrOfPinnedObject());
                }
                finally
                {
                    gch.Free();
                }             
            });
        }

        public async Task AddEntry(T obj)
        {

            await Task.Run(() =>
            {
                var ret = _delegateMethod(obj);
                var utf16Bytes = Encoding.Unicode.GetBytes(ret.Value + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    resultsPtr = ExternalMethods.AddEntryUTF16(_searchEngine, ret.Key, gch.AddrOfPinnedObject());
                }
                finally
                {
                    gch.Free();
                }
            });
        }


        public async Task<int[]> FindWithin(string wyrazenie)
        {
            return await Task.Run(() =>
            {
                int length;
                var utf16Bytes = Encoding.Unicode.GetBytes(wyrazenie + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    resultsPtr = ExternalMethods.FindUTF16(_searchEngine, gch.AddrOfPinnedObject(), out length, WordMatchMethod.Within);
                }
                finally
                {
                    gch.Free();
                }

                int[] results = new int[length];
                Marshal.Copy(resultsPtr, results, 0, length);
                Marshal.FreeCoTaskMem(resultsPtr);
                return results;
            });
        }

        public async Task<int[]> FindExact(string wyrazenie)
        {
            return await Task.Run(() =>
            {
                int length;
                var utf16Bytes = Encoding.Unicode.GetBytes(wyrazenie + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    resultsPtr = ExternalMethods.FindUTF16(_searchEngine, gch.AddrOfPinnedObject(), out length, WordMatchMethod.Exact);
                }
                finally
                {
                    gch.Free();
                }

                int[] results = new int[length];
                Marshal.Copy(resultsPtr, results, 0, length);
                Marshal.FreeCoTaskMem(resultsPtr);
                return results;
            });
        }

    }
}