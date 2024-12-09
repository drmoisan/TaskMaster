using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable
{
    public class SloLinkedList<T>:LockingObservableLinkedList<T>, ISmartSerializable<SloLinkedList<T>>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public SloLinkedList(): base() { ism = new SmartSerializable<SloLinkedList<T>>(this); }
        public SloLinkedList(IEnumerable<T> collection) : base(collection) { ism = new SmartSerializable<SloLinkedList<T>>(this); }

        #endregion ctor

        #region ISmartSerializable

        public NewSmartSerializableConfig Config { get => ism.Config; set => ism.Config = value; }
        protected SmartSerializable<SloLinkedList<T>> ism;

        public void Serialize() => ism.Serialize();
        public void Serialize(string filePath) => ism.Serialize(filePath);
        public void SerializeThreadSafe(string filePath) => ism.SerializeThreadSafe(filePath);
        public SloLinkedList<T> Deserialize(string fileName, string folderPath) => ism.Deserialize(fileName, folderPath);
        public SloLinkedList<T> Deserialize(string fileName, string folderPath, bool askUserOnError) => ism.Deserialize(fileName, folderPath, askUserOnError);
        public SloLinkedList<T> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);
        public async Task<SloLinkedList<T>> DeserializeAsync<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new() => await ism.DeserializeAsync(config);
        public async Task<SloLinkedList<T>> DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError) where U : class, ISmartSerializable<U>, new() => await ism.DeserializeAsync(config, askUserOnError);


        public string Name { get; set; }

        #endregion ISmartSerializable

        #region INotifyPropertyChanged

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void Notify([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        public static class Static
        {
            private static SmartSerializable<SloLinkedList<T>> GetInstance() => new();

            public static SloLinkedList<T> Deserialize(string fileName, string folderPath) =>
                GetInstance().Deserialize(fileName, folderPath);

            public static SloLinkedList<T> Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError);

            public static SloLinkedList<T> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);

            public static async Task<SloLinkedList<T>> DeserializeAsync<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config);

            public static async Task<SloLinkedList<T>> DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError) where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config, askUserOnError);

        }
    }
}
