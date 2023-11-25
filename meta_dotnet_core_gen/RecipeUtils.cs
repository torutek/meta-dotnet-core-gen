// ******************************************************************************************************************************
// Filename:    RecipeUtils.cs
// Description: Various utility methods to generate bitbake recipes.
// ******************************************************************************************************************************
// Copyright © Richard Dunkley 2023
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ******************************************************************************************************************************
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace meta_dotnet_core_gen
{
	public static class RecipeUtils
	{
		/// <summary>
		///   Downloads a file.
		/// </summary>
		/// <param name="link">Link to the file to be downloaded.</param>
		/// <param name="tempFilePath">Temporary file path to store the file to.</param>
		public static void DownloadFile(string link, string tempFilePath)
		{
			if (File.Exists(tempFilePath))
				File.Delete(tempFilePath);

			using (var client = new WebClient())
			{
				Thread.Sleep(1000);
				client.DownloadFile(link, tempFilePath);
			}
		}

		/// <summary>
		///   Computes the SHA512, SHA256, and MD5 hashes from the specified file.
		/// </summary>
		/// <param name="filePath">File to compute the hashes of.</param>
		/// <param name="computeMd5">True to compute the MD5, false to return null for <paramref name="md5Hash"/>.</param>
		/// <param name="computeSha256">True to compute the SHA256, false to return null for <paramref name="sha256Hash"/>.</param>
		/// <param name="sha256Hash">SHA256 computed hash. Null if <paramref name="computeSha256"/> is false.</param>
		/// <param name="md5Hash">MD5 computed hash. Null if <paramref name="computeMd5"/> is false.</param>
		/// <returns>SHA512 computed from the file.</returns>
		public static string ComputeHashes(string filePath, bool computeMd5, bool computeSha256, out string sha256Hash, out string md5Hash)
		{
			// Read all the data from the file.
			byte[] data = File.ReadAllBytes(filePath);

			sha256Hash = null;
			md5Hash = null;

			if(computeMd5)
			{
				using (MD5 md5 = MD5.Create())
				{
					md5Hash = ComputeHash(md5, data);
				}
			}

			if(computeSha256)
			{
				using (SHA256 sha = SHA256.Create())
				{
					sha256Hash = ComputeHash(sha, data);
				}
			}

			// Compute the SHA512.
			using (SHA512 sha = SHA512.Create())
			{
				return ComputeHash(sha, data);
			}
		}

		/// <summary>
		///   Computes the hash based on the provided <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <param name="hashAlgorithm">Algorithm to compute the hash with.</param>
		/// <param name="data">Data to compute the hash of.</param>
		/// <returns>Hexadecimal string of the hash.</returns>
		private static string ComputeHash(HashAlgorithm hashAlgorithm, byte[] data)
		{
			// Compute the hash.
			byte[] hash = hashAlgorithm.ComputeHash(data);

			var sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("x2"));
			return sb.ToString();
		}


	}
}
