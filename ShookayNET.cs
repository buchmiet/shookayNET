using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace shookayNET
{



    internal static partial  class ExternalMethods
    {
        [LibraryImport("shookay")]     
        public static partial IntPtr CreateSearchEngine();

        [LibraryImport("shookay")]
        public static partial void DeliverEntriesUTF16(IntPtr engine, IntPtr dataPointer, int length);

        [LibraryImport("shookay")]
        public static partial IntPtr FindWithinUTF16(IntPtr searchEngine, IntPtr wyrazenie, out int length);
        [LibraryImport("shookay")]
        public static partial IntPtr FindExactUTF16(IntPtr searchEngine, IntPtr wyrazenie, out int length);
 
        [LibraryImport("shookay")]
        public static partial void DeliverEntriesUTF16WithCallback(IntPtr engine, IntPtr dataPointer, int length, Commons.ProgressCallback progressCallback);
        [LibraryImport("shookay")]
        public static partial void FindExactUTF16WithCallback(IntPtr engine, IntPtr dataPointer, Commons.ProgressCallback progressCallback);
        [LibraryImport("shookay")]
        public static partial void FindWithinUTF16WithCallBack(IntPtr engine, IntPtr dataPointer, Commons.ProgressCallback progressCallback);

    }

    public static class Commons
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProgressCallback(int progress);
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

        public async Task DeliverEntries()
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
                    ExternalMethods.DeliverEntriesUTF16(_searchEngine, gch.AddrOfPinnedObject(), dane.Length);
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
                  
                    ExternalMethods.DeliverEntriesUTF16WithCallback(_searchEngine, gch.AddrOfPinnedObject(), dane.Length,progressCallback);
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
                    resultsPtr = ExternalMethods.FindWithinUTF16(_searchEngine, gch.AddrOfPinnedObject(), out length);
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

        public async Task FindWithinWithProgress(string wyrazenie,Commons.ProgressCallback progressCallback)
        {
            await Task.Run(() =>
            {
                int length;
                var utf16Bytes = Encoding.Unicode.GetBytes(wyrazenie + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    ExternalMethods.FindWithinUTF16WithCallBack(_searchEngine, gch.AddrOfPinnedObject(), progressCallback);
                }
                finally
                {
                    gch.Free();
                }              
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
                    resultsPtr = ExternalMethods.FindExactUTF16(_searchEngine, gch.AddrOfPinnedObject(), out length);
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

        public async Task FindExactWithProgress(string wyrazenie, Commons.ProgressCallback progressCallback)
        {
            await Task.Run(() =>
            {
                int length;
                var utf16Bytes = Encoding.Unicode.GetBytes(wyrazenie + "\0");
                IntPtr resultsPtr;
                GCHandle gch = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
                try
                {
                    ExternalMethods.FindExactUTF16WithCallback(_searchEngine, gch.AddrOfPinnedObject(), progressCallback);
                }
                finally
                {
                    gch.Free();
                }
            });
        }

    }
}