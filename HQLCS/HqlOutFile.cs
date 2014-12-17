using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hql
{
    class HqlOutFile : IDisposable
    {
        ///////////////////////
        // Static Functions

        static public string ReplaceInvalidFilenameCharacters(string filename)
        {
            return filename
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace(':', '_')
                .Replace('*', '_')
                .Replace('?', '_')
                .Replace('"', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('|', '_')
                .Replace(' ', '_')
                ;
        }

        static public string MakeSafeFilename(string filename)
        {
            string directory = System.IO.Path.GetDirectoryName(filename);
            string file = System.IO.Path.GetFileName(filename);
            string result = directory + (directory.Length == 0 ? "" : "\\") + ReplaceInvalidFilenameCharacters(file);
            return result;
        }

        ///////////////////////
        // Constructors

        public HqlOutFile(string filename)
        {
            Filename = filename;
            Open(false);
        }

        ///////////////////////
        // Overridden functions

        ///////////////////////
        // Public 

        public void Close()
        {
            if (_sw != null)
            {
                _sw.Close();
                _sw.Dispose();
                _sw = null;
            }
        }

        public void WriteLine(string s)
        {
            if (_sw == null)
                Open(true);
            _sw.WriteLine(s);
        }

        public void Cleanup()
        {
            if (_sw != null)
            {
                _sw.Close();
                _sw.Dispose();
                _sw = null;
            }
        }

        ///////////////////////
        // Private

        private void Open(bool append)
        {
            _sw = new StreamWriter(Filename, append);
            if (_sw == null)
                throw new Exception(String.Format("Unable to open {0} for writing", Filename));
        }

        void IDisposable.Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ///////////////////////
        // Fields

        public bool IsOpen
        {
            get { return (_sw != null); }
        }

        ///////////////////////
        // Getters/Setters

        public string Filename
        {
            get { return _filename; }
            set { _filename = MakeSafeFilename(value); }
        }
       
        ///////////////////////
        // Variables

        StreamWriter _sw;
        string _filename;
    }
}
