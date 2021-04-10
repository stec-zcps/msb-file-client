using System;

namespace msb_file_client
{
    public static class Packaging
    {
        public static string EncodeBase64(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        public static byte[] DecodeBase64(string value)
        {
            return Convert.FromBase64String(value);
        }

        public static void CopyTo(System.IO.Stream src, System.IO.Stream dest) {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(str);

            using (var msi = new System.IO.MemoryStream(bytes))
            using (var mso = new System.IO.MemoryStream()) {
                using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionMode.Compress)) {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes) {
            using (var msi = new System.IO.MemoryStream(bytes))
            using (var mso = new System.IO.MemoryStream()) {
                using (var gs = new System.IO.Compression.GZipStream(msi, System.IO.Compression.CompressionMode.Decompress)) {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return System.Text.Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}