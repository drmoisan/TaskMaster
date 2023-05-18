using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;

namespace UtilitiesVB
{



    [Serializable]
    public class SerializableList2<T> : IList<T>
    {

        private List<T> _innerList;
        private IEnumerable<T> _lazyLoader;
        private string _filename;
        private string _folderpath;
        private string _filepath;

        public SerializableList2()
        {
            _innerList = new List<T>();
        }

        public SerializableList2(List<T> listOfT)
        {
            _innerList = listOfT;
        }

        public SerializableList2(IEnumerable<T> IEnumerableOfT)
        {
            _lazyLoader = IEnumerableOfT;
        }

        public void Serialize()
        {
            if (!string.IsNullOrEmpty(Filepath))
            {
                Serialize(Filepath);
            }
        }

        public void Serialize(string filepath)
        {
            Filepath = filepath;

            throw new NotImplementedException();
        }

        public void ToCSV(string filepath)
        {

        }

        // Public Sub Deserialize()
        // If Filepath <> "" Then
        // Deserialize(Filepath)
        // End If
        // End Sub
        // 
        // Public Sub Deserialize(filepath As String)
        // Dim _csvSerializer = New CsvSerializer.Serializer
        // Dim listObj As Object = Nothing
        // Dim shouldExecute As Boolean = True
        // Me.Filepath = filepath
        // Try
        // Using csvStream As New FileStream(path:=filepath, mode:=FileMode.Open)
        // listObj = _csvSerializer.Deserialize(csvStream)
        // End Using
        // Catch ex As Exception
        // MsgBox("Error accessing file." & ex.Message)
        // shouldExecute = False
        // End Try
        // If shouldExecute Then
        // _innerList = TryCast(listObj, List(Of T))
        // If _innerList Is Nothing Then
        // MsgBox("Cannot convert file " & filepath & "to List(Of T)")
        // End If
        // End If

        // End Sub

        private void ensureList()
        {
            if (_innerList is null)
                _innerList = new List<T>(_lazyLoader);
        }

        public int IndexOf(T item)
        {
            ensureList();
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ensureList();
            _innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ensureList();
            _innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                ensureList();
                return _innerList[index];
            }
            set
            {
                ensureList();
                _innerList[index] = value;
            }
        }

        public void Add(T item)
        {
            ensureList();
            _innerList.Add(item);
        }

        public void Clear()
        {
            ensureList();
            _innerList.Clear();
        }

        public bool Contains(T item) // Implements ICollection(Of T).Contains
        {
            ensureList();
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ensureList();
            _innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                ensureList();
                return _innerList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public string Filepath
        {
            get
            {
                if (string.IsNullOrEmpty(_filepath))
                {
                    if (string.IsNullOrEmpty(_filename) & string.IsNullOrEmpty(_folderpath))
                    {
                        Interaction.MsgBox("Filepath is empty");
                    }
                    else if (string.IsNullOrEmpty(_filename))
                    {
                        Interaction.MsgBox("Folderpath has a value but Filename is empty");
                    }
                    else
                    {
                        Interaction.MsgBox("Filename has a value but Folderpath is empty");
                    }
                }
                return _filepath;
            }
            set
            {
                _filepath = value;
                _folderpath = Path.GetDirectoryName(_filepath);
                _filename = Path.GetFileName(_filepath);
            }
        }

        public string Folderpath
        {
            get
            {
                return _folderpath;
            }
            set
            {
                _folderpath = value;
                if (!string.IsNullOrEmpty(_filename))
                {
                    _filepath = Path.Combine(_folderpath, _filename);
                }
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (!string.IsNullOrEmpty(_folderpath))
                {
                    _filepath = Path.Combine(_folderpath, _filename);
                }
            }
        }

        public bool Remove(T item)
        {
            ensureList();
            return _innerList.Remove(item);
        }

        public IEnumerator GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }

        private IEnumerator<T> IEnumerable_GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => IEnumerable_GetEnumerator();


    }
}