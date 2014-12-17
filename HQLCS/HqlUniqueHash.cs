using System;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlUniqueHash
    {
        static object lockit = new object();

        static HqlUniqueHash _hash;
        static public HqlUniqueHash GetUniqueHash()
        {
            if (_hash == null)
            {
                lock (lockit)
                {
                    if (_hash == null) _hash = new HqlUniqueHash();
                }
                return GetUniqueHash();
            }

            return _hash;
        }

        static public int CalculateHashCode(HqlKey key)
        {
            int h1 = GetUniqueHash()._CalculateHashCode(key);
            return h1;
        }

        private HqlUniqueHash()
        {
            // Help file:
            // A smaller load factor (0.1 - 1.0) means faster lookup at the cost of increased memory consumption.
            // A load factor of 1.0 is the best balance between speed and size.
            _ht = new Dictionary<int, HqlKey>(10000);
        }

        private int _CalculateHashCode(HqlKey key)
        {
            string sbstr = key.ToHashString();
            int h1 = lookup3ycs(sbstr);
            long modular = 0;
            for (; ; )
            {
                if (!_ht.ContainsKey(h1))
                {
                    _ht[h1] = key;
                    break;
                }
                if (_ht[h1].Equals(key))
                    break;
                
                // HASH COLLISION!!
                if (modular == 0)
                    modular = lookup3ycs(sbstr + "\x01" + sbstr);
                long newlong = (((long)h1 + modular) % (long)Int32.MaxValue);
                h1 = (int)newlong;
            }
            return h1;
        }

        static private int lookup3ycs(string s)
        {
            /* <p>The hash value of a character sequence is defined to be the hash of
            * it's unicode code points, according to {@link #lookup3ycs(int[] k, int offset, int length, int initval)}
            * </p>
            * <p>If you know the number of code points in the {@code CharSequence}, you can
            * generate the same hash as the original lookup3
            * via {@code lookup3ycs(s, start, end, initval+(numCodePoints<<2))}
            */
            //
            int start = 0;
            int end = s.Length;
            Int64 a, b, c;
            a = b = c = 0x0eadbeef;// +(uint)initval;
            // only difference from lookup3 is that "+ (length<<2)" is missing
            // since we don't know the number of code points to start with,
            // and don't want to have to pre-scan the string to find out.

            try
            {
                int i = start;
                bool mixed = true;  // have the 3 state variables been adequately mixed?
                for (; ; )
                {
                    if (i >= 40)
                    {
                        i = i + 1 - 1;
                    }

                    if (i >= end)
                        break;
                    mixed = false;
                    char ch;
                    ch = s[i++];
                    a += Char.IsHighSurrogate(ch) && i < end ? Char.ConvertToUtf32(ch, s[i++]) : ch;
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    if (i >= end) break;
                    ch = s[i++];
                    b += Char.IsHighSurrogate(ch) && i < end ? Char.ConvertToUtf32(ch, s[i++]) : ch;
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    if (i >= end) break;
                    ch = s[i++];
                    c += Char.IsHighSurrogate(ch) && i < end ? Char.ConvertToUtf32(ch, s[i++]) : ch;
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    if (i >= end) break;

                    {
                        //a -= c; a ^= (c << 4) | (c >> -4); c += b;
                        //b -= a; b ^= (a << 6) | (a >> -6); a += c;
                        //c -= b; c ^= (b << 8) | (b >> -8); b += a;
                        //a -= c; a ^= (c << 16) | (c >> -16); c += b;
                        //b -= a; b ^= (a << 19) | (a >> -19); a += c;
                        //c -= b; c ^= (b << 4) | (b >> -4); b += a;

                        //a -= c; a ^= funny_shift(c, 4); c += b;
                        //b -= a; b ^= funny_shift(a, 6); a += c;
                        //c -= b; c ^= funny_shift(b, 8); b += a;
                        //a -= c; a ^= funny_shift(c, 16); c += b;
                        //b -= a; b ^= funny_shift(a, 19); a += c;
                        //c -= b; c ^= funny_shift(b, 4); b += a;

                        a = sub(a, c); a ^= funny_shift(c, 4); c = add(c, b);
                        //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                        if (i == 48)
                        {
                            i = i + 1 - 1;
                        }
                        b = sub(b, a); b ^= funny_shift(a, 6); a = add(a, c);
                        //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                        c = sub(c, b); c ^= funny_shift(b, 8); b = add(b, a);
                        //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                        a = sub(a, c); a ^= funny_shift(c, 16); c = add(c, b);
                        //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                        b = sub(b, a); b ^= funny_shift(a, 19); a = add(a, c);
                        //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                        c = sub(c, b); c ^= funny_shift(b, 4); b = add(b, a);
                    }
                    mixed = true;
                }
                //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);

                if (!mixed)
                {
                    // final(a,b,c)
                    //c ^= b; c -= (b << 14) | (b >> -14);
                    //a ^= c; a -= (c << 11) | (c >> -11);
                    //b ^= a; b -= (a << 25) | (a >> -25);
                    //c ^= b; c -= (b << 16) | (b >> -16);
                    //a ^= c; a -= (c << 4) | (c >> -4);
                    //b ^= a; b -= (a << 14) | (a >> -14);
                    //c ^= b; c -= (b << 24) | (b >> -24);

                    c ^= b; c = sub(c, funny_shift(b, 14));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    a ^= c; a = sub(a, funny_shift(c, 11));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    b ^= a; b = sub(b, funny_shift(a, 25));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    c ^= b; c = sub(c, funny_shift(b, 16));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    a ^= c; a = sub(a, funny_shift(c, 4));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    b ^= a; b = sub(b, funny_shift(a, 14));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                    c ^= b; c = sub(c, funny_shift(b, 24));
                    //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
                }
                //Console.WriteLine("{0}|{1}|{2}|{3}|", i, a, b, c);
            }
            catch (Exception ex)
            {
                string s2 = ex.ToString();
                Console.WriteLine("Overflow" + s2);
            }

            //Console.WriteLine("{0}|{1}|{2}|{3}|fin|", -1, a, b, c);
            return (int)(c % Int32.MaxValue);
        }

        // TODO, make a smarter function, catches are expensive!!
        static private long sub(long n1, long n2)
        {
            try
            {
                if ((n1 - n2) < Int32.MinValue || (n1 - n2) > Int32.MaxValue)
                {
                    return (n1 % Int32.MaxValue) - (n2 % Int32.MaxValue);
                }
                return n1 - n2;
            }
            catch
            {
                return (n1 % Int32.MaxValue) - (n2 % Int32.MaxValue);
            }
        }

        // TODO, make a smarter function, catches are expensive!!
        static private long add(long n1, long n2)
        {
            try
            {
                if ((n1 + n2) < Int32.MinValue || (n1 + n2) > Int32.MaxValue)
                {
                    return (n1 % Int32.MaxValue) + (n2 % Int32.MaxValue);
                }
                return n1 + n2;
            }
            catch
            {
                return (n1 % Int32.MaxValue) + (n2 % Int32.MaxValue);
            }
        }

        static private long funny_shift(long dest, int n)
        {
            long left = dest << n;
            long right = (dest > 0) ? dest >> (0 - n) : (0 - dest) >> (0 - n);

            long res = left | right;
            res = res % Int32.MaxValue;

            return res;
        }

        Dictionary<int, HqlKey> _ht;        
    }
}
