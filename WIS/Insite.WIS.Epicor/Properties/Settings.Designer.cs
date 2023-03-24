
namespace Insite.WIS.Epicor.Properties {


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default {
            get {
                return defaultInstance;
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.1.98.33/EpicorServices/BomSearchService.asmx")]
        public string Insite_WindowsIntegrationPlugins_Epicor_Epicor9BomSearchService_BomSearchService {
            get {
                return ((string)(this["Insite_WindowsIntegrationPlugins_Epicor_Epicor9BomSearchService_BomSearchService"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.1.98.33/EpicorServices/ConfiguratorService.asmx")]
        public string Insite_WindowsIntegrationPlugins_Epicor_Epicor9ConfiguratorService_ConfiguratorService {
            get {
                return ((string)(this["Insite_WindowsIntegrationPlugins_Epicor_Epicor9ConfiguratorService_ConfiguratorSe" +
                    "rvice"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.1.98.33/EpicorServices/DynamicQueryService.asmx")]
        public string Insite_WindowsIntegrationPlugins_Epicor_Epicor9DynamicQueryService_DynamicQueryService {
            get {
                return ((string)(this["Insite_WindowsIntegrationPlugins_Epicor_Epicor9DynamicQueryService_DynamicQuerySe" +
                    "rvice"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.1.98.33/EpicorServices/PartService.asmx")]
        public string Insite_WindowsIntegrationPlugins_Epicor_Epicor9PartService_PartService {
            get {
                return ((string)(this["Insite_WindowsIntegrationPlugins_Epicor_Epicor9PartService_PartService"]));
            }
        }
    }
}