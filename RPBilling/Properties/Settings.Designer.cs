﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RackPeople.BillingAPI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("https://nav.gowingu.net:7047/DynamicsNAV90/WS/Rackpeople%20Consulting%20ApS/Page/" +
            "CustomerInfo2?tenant=rackpeople")]
        public string BillingAPI_NAVCustomerInfoRef_CustomerInfo2_Service {
            get {
                return ((string)(this["BillingAPI_NAVCustomerInfoRef_CustomerInfo2_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://nav.gowingu.net:7047/DynamicsNAV90/WS/Rackpeople%20Consulting%20ApS/Page/" +
            "Vareoversigt?tenant=rackpeople")]
        public string BillingAPI_NAVProductService_Vareoversigt_Service {
            get {
                return ((string)(this["BillingAPI_NAVProductService_Vareoversigt_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://nav.gowingu.net:7047/DynamicsNAV90/WS/Rackpeople%20Consulting%20ApS/Page/" +
            "SalesInvoice_Service?tenant=rackpeople")]
        public string RackPeople_BillingAPI_NAVSalesInvoiceService_SalesInvoice_Service_Service {
            get {
                return ((string)(this["RackPeople_BillingAPI_NAVSalesInvoiceService_SalesInvoice_Service_Service"]));
            }
        }
    }
}
