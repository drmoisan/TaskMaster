using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.AppGlobals
{
    public class ManagerLazyConfig: SmartSerializable<ManagerLazyConfig>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ManagerLazyConfig() { }
        
        public List<ManagerLazyConfigStruct> Configurations { get; set; }
        
        private JsonSerializerSettings GetSettings()
        {
            var settings = GetDefaultSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;            
            return settings;
        }

        internal ManagerLazyConfig DeserializeConfig(byte[] binary)
        {
            var jsonObject = TryConvertBinaryToJson(binary);
            if (jsonObject.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                return DeserializeConfig(jsonObject);
            }
        }

        internal ManagerLazyConfig DeserializeConfig(string jsonObject)
        {
            var settings = GetSettings();
            ManagerLazyConfig instance = null;
            try
            {
                instance = JsonConvert.DeserializeObject<ManagerLazyConfig>(
                    jsonObject, settings);
            }
            catch (Exception e)
            {
                logger.Error($"Error in {nameof(DeserializeConfig)}.\n{e.Message}", e);
                return null;
            }

            return instance;
        }

        internal string TryConvertBinaryToJson(byte[] binary)
        {
            try
            {
                var jsonObject = System.Text.Encoding.UTF8.GetString(binary);
                return jsonObject;
            }
            catch (Exception e)
            {
                logger.Error($"Error in {nameof(TryConvertBinaryToJson)}.\n{e.Message}", e);
                return null;
            }
        }

    }

    public struct ManagerLazyConfigStruct
    {
        public string ResourceName { get; set; }
        public string ClassifierName { get; set; }
        public bool Active { get; set; }
    }
}
