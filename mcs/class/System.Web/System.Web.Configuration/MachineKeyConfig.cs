//
// System.Web.Configuration.MachineKeyConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Configuration;
using System.Xml;
using System.Security.Cryptography;

namespace System.Web.Configuration
{
	class MachineKeyConfig
	{
		byte [] validation_key;
		//bool    isolate_validation;
		byte [] decryption_key;
		byte [] decryption_key_192bits;
		//bool    isolate_decryption; // For us, this is always true by now.
		MachineKeyValidation validation_type;

		static byte [] autogenerated;
		static byte [] autogenerated_decrypt;
		
		static MachineKeyConfig ()
		{
			autogenerated = new byte [64];
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			rng.GetBytes (autogenerated);
			autogenerated_decrypt = new byte [64];
			rng.GetBytes (autogenerated_decrypt);
		}
		
		internal MachineKeyConfig (object parent)
		{
			if (parent is MachineKeyConfig) {
				MachineKeyConfig p = (MachineKeyConfig) parent;
				validation_key = p.validation_key;
				decryption_key = p.decryption_key;
				validation_type = p.validation_type;
			}
		}

		static byte ToHexValue (char c, bool high)
		{
			byte v;
			if (c >= '0' && c <= '9')
				v = (byte) (c - '0');
			else if (c >= 'a' && c <= 'f')
				v = (byte) (c - 'a' + 10);
			else if (c >= 'A' && c <= 'F')
				v = (byte) (c - 'A' + 10);
			else
				throw new ArgumentException ("Invalid hex character");

			if (high)
				v <<= 4;

			return v;
		}
		
		internal static byte [] GetBytes (string key, int len)
		{
			byte [] result = new byte [len / 2];
			for (int i = 0; i < len; i += 2)
				result [i / 2] = (byte) (ToHexValue (key [i], true) + ToHexValue (key [i + 1], false));

			return result;
		}

		static byte [] MakeKey (string key, bool decryption) //, out bool isolate)
		{
			if (key == null || key.StartsWith ("AutoGenerate")){
				//isolate = key.IndexOf ("IsolateApps") != 1;

				return (decryption) ? autogenerated_decrypt : autogenerated;
			}

			//isolate = false;

			int len = key.Length;
			if (len < 40 || len > 128 || (len % 2) == 1)
				throw new ArgumentException ("Invalid key length");

			return GetBytes (key, len);
		}
		
		internal void SetValidationKey (string n)
		{
			validation_key = MakeKey (n, false); //, out isolate_validation);
		}
		
		internal byte [] ValidationKey {
			get { return validation_key; }
		}

		internal void SetDecryptionKey (string n)
		{
			decryption_key = MakeKey (n, true); //, out isolate_decryption);
			decryption_key_192bits = new byte [24];
			int count = 24;
			if (decryption_key.Length < 24)
				count = decryption_key.Length;
			Buffer.BlockCopy (decryption_key, 0, decryption_key_192bits, 0, count);
		}
		
		internal byte [] DecryptionKey {
			get { return decryption_key; }
		}

		internal byte [] DecryptionKey192Bits {
			get { return decryption_key_192bits; }
		}

		internal MachineKeyValidation ValidationType {
			get {
				return validation_type;
			}
			set {
				validation_type = value;
			}
		}
	}
}

