namespace Insite.WIS.Sx;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class LoginRequest { }

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class LoginResponse
{
    private string errorMessageField;

    private string userTokenField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public string userToken
    {
        get { return this.userTokenField; }
        set { this.userTokenField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "ARCustomerMnt.input.FieldModification",
    Namespace = "Nxtrend.WS"
)]
public partial class ARCustomerMntinputFieldModification
{
    private int setNumberField;

    private int sequenceNumberField;

    private string key1Field;

    private string key2Field;

    private string updateModeField;

    private string fieldNameField;

    private string fieldValueField;

    /// <remarks/>
    public int setNumber
    {
        get { return this.setNumberField; }
        set { this.setNumberField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public string key1
    {
        get { return this.key1Field; }
        set { this.key1Field = value; }
    }

    /// <remarks/>
    public string key2
    {
        get { return this.key2Field; }
        set { this.key2Field = value; }
    }

    /// <remarks/>
    public string updateMode
    {
        get { return this.updateModeField; }
        set { this.updateModeField = value; }
    }

    /// <remarks/>
    public string fieldName
    {
        get { return this.fieldNameField; }
        set { this.fieldNameField = value; }
    }

    /// <remarks/>
    public string fieldValue
    {
        get { return this.fieldValueField; }
        set { this.fieldValueField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARCustomerMntRequest
{
    private ARCustomerMntinputFieldModification[] arrayFieldModificationField;

    private string extraDataField;

    /// <remarks/>
    public ARCustomerMntinputFieldModification[] arrayFieldModification
    {
        get { return this.arrayFieldModificationField; }
        set { this.arrayFieldModificationField = value; }
    }

    /// <remarks/>
    public string extraData
    {
        get { return this.extraDataField; }
        set { this.extraDataField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARCustomerMntResponse
{
    private string errorMessageField;

    private string returnDataField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public string returnData
    {
        get { return this.returnDataField; }
        set { this.returnDataField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARGetCustomerDataGeneralRequest
{
    private double customerNumberField;

    private string shipToField;

    /// <remarks/>
    public double customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string shipTo
    {
        get { return this.shipToField; }
        set { this.shipToField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARGetCustomerDataGeneralResponse
{
    private string errorMessageField;

    private string nameField;

    private string address1Field;

    private string address2Field;

    private string cityField;

    private string stateField;

    private string zipCdField;

    private string countryCodeField;

    private string countryDescField;

    private string phoneField;

    private string faxField;

    private string statusTypeField;

    private string commentField;

    private int sicCd1Field;

    private int sicCd2Field;

    private int sicCd3Field;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string zipCd
    {
        get { return this.zipCdField; }
        set { this.zipCdField = value; }
    }

    /// <remarks/>
    public string countryCode
    {
        get { return this.countryCodeField; }
        set { this.countryCodeField = value; }
    }

    /// <remarks/>
    public string countryDesc
    {
        get { return this.countryDescField; }
        set { this.countryDescField = value; }
    }

    /// <remarks/>
    public string phone
    {
        get { return this.phoneField; }
        set { this.phoneField = value; }
    }

    /// <remarks/>
    public string fax
    {
        get { return this.faxField; }
        set { this.faxField = value; }
    }

    /// <remarks/>
    public string statusType
    {
        get { return this.statusTypeField; }
        set { this.statusTypeField = value; }
    }

    /// <remarks/>
    public string comment
    {
        get { return this.commentField; }
        set { this.commentField = value; }
    }

    /// <remarks/>
    public int sicCd1
    {
        get { return this.sicCd1Field; }
        set { this.sicCd1Field = value; }
    }

    /// <remarks/>
    public int sicCd2
    {
        get { return this.sicCd2Field; }
        set { this.sicCd2Field = value; }
    }

    /// <remarks/>
    public int sicCd3
    {
        get { return this.sicCd3Field; }
        set { this.sicCd3Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARGetShipToListRequest
{
    private double customerNumberField;

    private string shipToField;

    private string phoneNumberField;

    private string nameField;

    private string cityField;

    private string stateField;

    private string postalCodeField;

    private bool includeClosedJobsField;

    private string sortField;

    private int recordLimitField;

    /// <remarks/>
    public double customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string shipTo
    {
        get { return this.shipToField; }
        set { this.shipToField = value; }
    }

    /// <remarks/>
    public string phoneNumber
    {
        get { return this.phoneNumberField; }
        set { this.phoneNumberField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public bool includeClosedJobs
    {
        get { return this.includeClosedJobsField; }
        set { this.includeClosedJobsField = value; }
    }

    /// <remarks/>
    public string sort
    {
        get { return this.sortField; }
        set { this.sortField = value; }
    }

    /// <remarks/>
    public int recordLimit
    {
        get { return this.recordLimitField; }
        set { this.recordLimitField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class ARGetShipToListResponse
{
    private string errorMessageField;

    private bool moreRecordsAvailableField;

    private ARGetShipToListoutputShipto[] arrayShiptoField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public bool moreRecordsAvailable
    {
        get { return this.moreRecordsAvailableField; }
        set { this.moreRecordsAvailableField = value; }
    }

    /// <remarks/>
    public ARGetShipToListoutputShipto[] arrayShipto
    {
        get { return this.arrayShiptoField; }
        set { this.arrayShiptoField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "ARGetShipToList.output.Shipto",
    Namespace = "Nxtrend.WS"
)]
public partial class ARGetShipToListoutputShipto
{
    private double customerNumberField;

    private string shipToField;

    private string nameField;

    private string address1Field;

    private string address2Field;

    private string cityField;

    private string stateField;

    private string postalCodeField;

    private string phoneNumberField;

    private string notesFlagField;

    private string sortFieldField;

    /// <remarks/>
    public double customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string shipTo
    {
        get { return this.shipToField; }
        set { this.shipToField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public string phoneNumber
    {
        get { return this.phoneNumberField; }
        set { this.phoneNumberField = value; }
    }

    /// <remarks/>
    public string notesFlag
    {
        get { return this.notesFlagField; }
        set { this.notesFlagField = value; }
    }

    /// <remarks/>
    public string sortField
    {
        get { return this.sortFieldField; }
        set { this.sortFieldField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class OEFullOrderMntV5Request
{
    private OEFullOrderMntV5inputOrder[] arrayOrderField;

    private OEFullOrderMntV5inputCustomer[] arrayCustomerField;

    private OEFullOrderMntV5inputItem[] arrayItemField;

    private OEFullOrderMntV5inputShipFrom[] arrayShipFromField;

    private OEFullOrderMntV5inputShipTo[] arrayShipToField;

    private OEFullOrderMntV5inputBillTo[] arrayBillToField;

    private OEFullOrderMntV5inputTerms[] arrayTermsField;

    private OEFullOrderMntV5inputSchedule[] arrayScheduleField;

    private OEFullOrderMntV5inputTotal[] arrayTotalField;

    private OEFullOrderMntV5inputHeaderExtra[] arrayHeaderExtraField;

    private OEFullOrderMntV5inputLineExtra[] arrayLineExtraField;

    /// <remarks/>
    public OEFullOrderMntV5inputOrder[] arrayOrder
    {
        get { return this.arrayOrderField; }
        set { this.arrayOrderField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputCustomer[] arrayCustomer
    {
        get { return this.arrayCustomerField; }
        set { this.arrayCustomerField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputItem[] arrayItem
    {
        get { return this.arrayItemField; }
        set { this.arrayItemField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputShipFrom[] arrayShipFrom
    {
        get { return this.arrayShipFromField; }
        set { this.arrayShipFromField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputShipTo[] arrayShipTo
    {
        get { return this.arrayShipToField; }
        set { this.arrayShipToField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputBillTo[] arrayBillTo
    {
        get { return this.arrayBillToField; }
        set { this.arrayBillToField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputTerms[] arrayTerms
    {
        get { return this.arrayTermsField; }
        set { this.arrayTermsField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputSchedule[] arraySchedule
    {
        get { return this.arrayScheduleField; }
        set { this.arrayScheduleField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputTotal[] arrayTotal
    {
        get { return this.arrayTotalField; }
        set { this.arrayTotalField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputHeaderExtra[] arrayHeaderExtra
    {
        get { return this.arrayHeaderExtraField; }
        set { this.arrayHeaderExtraField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5inputLineExtra[] arrayLineExtra
    {
        get { return this.arrayLineExtraField; }
        set { this.arrayLineExtraField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class OEFullOrderMntV5Response
{
    private string errorMessageField;

    private OEFullOrderMntV5outputAcknowledgement[] arrayAcknowledgementField;

    private OEFullOrderMntV5outputHeader[] arrayHeaderField;

    private OEFullOrderMntV5outputItem[] arrayItemField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5outputAcknowledgement[] arrayAcknowledgement
    {
        get { return this.arrayAcknowledgementField; }
        set { this.arrayAcknowledgementField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5outputHeader[] arrayHeader
    {
        get { return this.arrayHeaderField; }
        set { this.arrayHeaderField = value; }
    }

    /// <remarks/>
    public OEFullOrderMntV5outputItem[] arrayItem
    {
        get { return this.arrayItemField; }
        set { this.arrayItemField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.BillTo",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputBillTo
{
    private string address1Field;

    private string address2Field;

    private string billToNumberField;

    private string cityField;

    private string contactField;

    private string countryCodeField;

    private string dunsNumberField;

    private string nameField;

    private string phoneField;

    private string postalCodeField;

    private string stateField;

    private string user1Field;

    private string user2Field;

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string billToNumber
    {
        get { return this.billToNumberField; }
        set { this.billToNumberField = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string contact
    {
        get { return this.contactField; }
        set { this.contactField = value; }
    }

    /// <remarks/>
    public string countryCode
    {
        get { return this.countryCodeField; }
        set { this.countryCodeField = value; }
    }

    /// <remarks/>
    public string dunsNumber
    {
        get { return this.dunsNumberField; }
        set { this.dunsNumberField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string phone
    {
        get { return this.phoneField; }
        set { this.phoneField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Customer",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputCustomer
{
    private string address1Field;

    private string address2Field;

    private string cityField;

    private string contactField;

    private string countryCodeField;

    private string customerNumberField;

    private string dunsNumberField;

    private string nameField;

    private string phoneField;

    private string postalCodeField;

    private string stateField;

    private string user1Field;

    private string user2Field;

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string contact
    {
        get { return this.contactField; }
        set { this.contactField = value; }
    }

    /// <remarks/>
    public string countryCode
    {
        get { return this.countryCodeField; }
        set { this.countryCodeField = value; }
    }

    /// <remarks/>
    public string customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string dunsNumber
    {
        get { return this.dunsNumberField; }
        set { this.dunsNumberField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string phone
    {
        get { return this.phoneField; }
        set { this.phoneField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Order",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputOrder
{
    private string actionTypeField;

    private string attentionField;

    private string batchNameField;

    private string backOrderFlagField;

    private string buyerField;

    private string cancelDateField;

    private string confirmFlagField;

    private string companyNumberField;

    private string contractNumberField;

    private string correlationDataField;

    private string currencyCodeField;

    private string directionField;

    private string expectedShipDateField;

    private string freightOnBoardDescriptionField;

    private string freightOnBoardFlagField;

    private string notesIndicatorField;

    private string operatorInitialsField;

    private string orderDispositionField;

    private string orderNumberField;

    private string orderSuffixField;

    private string partnerIdField;

    private string purchaseOrderIssueDateField;

    private string purchaseOrderNumberField;

    private string purchaseOrderSuffixField;

    private string promiseDateField;

    private string referenceField;

    private string releaseNumberField;

    private string requestedShipDateField;

    private string rushFlagField;

    private string shipInstructionsField;

    private string shipViaField;

    private string substitutionFlagField;

    private string transactionTypeField;

    private string user1Field;

    private string user2Field;

    private string user3Field;

    private string user4Field;

    private string warehouseField;

    private string user5Field;

    private string user6Field;

    private string user7Field;

    private string user8Field;

    private string user9Field;

    private string taxableFlagField;

    private double floorPlanningCustomerField;

    private string approvalTypeField;

    private string lostBusinessTypeField;

    private string salesRepInField;

    private string salesRepOutField;

    private string takenByField;

    private double addonAmount1Field;

    private double addonAmount2Field;

    private double addonAmount3Field;

    private double addonAmount4Field;

    private int addonNumber1Field;

    private int addonNumber2Field;

    private int addonNumber3Field;

    private int addonNumber4Field;

    private string addonType1Field;

    private string addonType2Field;

    private string addonType3Field;

    private string addonType4Field;

    private int addonTaxGroup1Field;

    private int addonTaxGroup2Field;

    private int addonTaxGroup3Field;

    private int addonTaxGroup4Field;

    private NxtDate billDateField;

    private double contactIdField;

    private string creditReasonTypeField;

    private string currencyTypeField;

    private int divisionNumberField;

    private NxtDate directRouteDeliveryDateField;

    private string directRouteDeliveryTimeField;

    private bool directRouteHoldFlagField;

    private double downPaymentAmountField;

    private int geoCodeField;

    private bool inboundFreightFlagField;

    private string jobNumberField;

    private string languageCodeField;

    private bool lockFlagField;

    private int longestLeadTimeDaysField;

    private double lumpBillingAmountField;

    private bool lumpBillingFlagField;

    private bool lumpBillingPriceFlagField;

    private string nonTaxTypeField;

    private bool outboundFreightFlagField;

    private double paymentAmount1Field;

    private double paymentAmount2Field;

    private double paymentAmount3Field;

    private bool printPricesOnPickTicketsField;

    private int priceCodeField;

    private bool reprintPickTicketField;

    private bool printPricesFlagField;

    private string pstLicenseNumberField;

    private double pstTaxAmountField;

    private string routeField;

    private string sourceProsField;

    private double specialDiscountAmountField;

    private string stagingAreaField;

    private string stateCodeField;

    private int standingOrderDaysField;

    private bool standingOrderTypeField;

    private string taxAuthorityField;

    private string taxDefaultTypeField;

    private double tenderAmountField;

    private double termsDiscountAmountField;

    private bool termsLineFlagField;

    private double termsPercentField;

    private string updateTypeField;

    private string wholeOrderDefaultTypeField;

    private double wholeOrderDiscountAmountField;

    private double wholeOrderDiscountPercentField;

    private string wholeOrderDiscountTypeField;

    private double writeOffAmountField;

    private string zoneField;

    /// <remarks/>
    public string actionType
    {
        get { return this.actionTypeField; }
        set { this.actionTypeField = value; }
    }

    /// <remarks/>
    public string attention
    {
        get { return this.attentionField; }
        set { this.attentionField = value; }
    }

    /// <remarks/>
    public string batchName
    {
        get { return this.batchNameField; }
        set { this.batchNameField = value; }
    }

    /// <remarks/>
    public string backOrderFlag
    {
        get { return this.backOrderFlagField; }
        set { this.backOrderFlagField = value; }
    }

    /// <remarks/>
    public string buyer
    {
        get { return this.buyerField; }
        set { this.buyerField = value; }
    }

    /// <remarks/>
    public string cancelDate
    {
        get { return this.cancelDateField; }
        set { this.cancelDateField = value; }
    }

    /// <remarks/>
    public string confirmFlag
    {
        get { return this.confirmFlagField; }
        set { this.confirmFlagField = value; }
    }

    /// <remarks/>
    public string companyNumber
    {
        get { return this.companyNumberField; }
        set { this.companyNumberField = value; }
    }

    /// <remarks/>
    public string contractNumber
    {
        get { return this.contractNumberField; }
        set { this.contractNumberField = value; }
    }

    /// <remarks/>
    public string correlationData
    {
        get { return this.correlationDataField; }
        set { this.correlationDataField = value; }
    }

    /// <remarks/>
    public string currencyCode
    {
        get { return this.currencyCodeField; }
        set { this.currencyCodeField = value; }
    }

    /// <remarks/>
    public string direction
    {
        get { return this.directionField; }
        set { this.directionField = value; }
    }

    /// <remarks/>
    public string expectedShipDate
    {
        get { return this.expectedShipDateField; }
        set { this.expectedShipDateField = value; }
    }

    /// <remarks/>
    public string freightOnBoardDescription
    {
        get { return this.freightOnBoardDescriptionField; }
        set { this.freightOnBoardDescriptionField = value; }
    }

    /// <remarks/>
    public string freightOnBoardFlag
    {
        get { return this.freightOnBoardFlagField; }
        set { this.freightOnBoardFlagField = value; }
    }

    /// <remarks/>
    public string notesIndicator
    {
        get { return this.notesIndicatorField; }
        set { this.notesIndicatorField = value; }
    }

    /// <remarks/>
    public string operatorInitials
    {
        get { return this.operatorInitialsField; }
        set { this.operatorInitialsField = value; }
    }

    /// <remarks/>
    public string orderDisposition
    {
        get { return this.orderDispositionField; }
        set { this.orderDispositionField = value; }
    }

    /// <remarks/>
    public string orderNumber
    {
        get { return this.orderNumberField; }
        set { this.orderNumberField = value; }
    }

    /// <remarks/>
    public string orderSuffix
    {
        get { return this.orderSuffixField; }
        set { this.orderSuffixField = value; }
    }

    /// <remarks/>
    public string partnerId
    {
        get { return this.partnerIdField; }
        set { this.partnerIdField = value; }
    }

    /// <remarks/>
    public string purchaseOrderIssueDate
    {
        get { return this.purchaseOrderIssueDateField; }
        set { this.purchaseOrderIssueDateField = value; }
    }

    /// <remarks/>
    public string purchaseOrderNumber
    {
        get { return this.purchaseOrderNumberField; }
        set { this.purchaseOrderNumberField = value; }
    }

    /// <remarks/>
    public string purchaseOrderSuffix
    {
        get { return this.purchaseOrderSuffixField; }
        set { this.purchaseOrderSuffixField = value; }
    }

    /// <remarks/>
    public string promiseDate
    {
        get { return this.promiseDateField; }
        set { this.promiseDateField = value; }
    }

    /// <remarks/>
    public string reference
    {
        get { return this.referenceField; }
        set { this.referenceField = value; }
    }

    /// <remarks/>
    public string releaseNumber
    {
        get { return this.releaseNumberField; }
        set { this.releaseNumberField = value; }
    }

    /// <remarks/>
    public string requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public string rushFlag
    {
        get { return this.rushFlagField; }
        set { this.rushFlagField = value; }
    }

    /// <remarks/>
    public string shipInstructions
    {
        get { return this.shipInstructionsField; }
        set { this.shipInstructionsField = value; }
    }

    /// <remarks/>
    public string shipVia
    {
        get { return this.shipViaField; }
        set { this.shipViaField = value; }
    }

    /// <remarks/>
    public string substitutionFlag
    {
        get { return this.substitutionFlagField; }
        set { this.substitutionFlagField = value; }
    }

    /// <remarks/>
    public string transactionType
    {
        get { return this.transactionTypeField; }
        set { this.transactionTypeField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }

    /// <remarks/>
    public string user3
    {
        get { return this.user3Field; }
        set { this.user3Field = value; }
    }

    /// <remarks/>
    public string user4
    {
        get { return this.user4Field; }
        set { this.user4Field = value; }
    }

    /// <remarks/>
    public string warehouse
    {
        get { return this.warehouseField; }
        set { this.warehouseField = value; }
    }

    /// <remarks/>
    public string user5
    {
        get { return this.user5Field; }
        set { this.user5Field = value; }
    }

    /// <remarks/>
    public string user6
    {
        get { return this.user6Field; }
        set { this.user6Field = value; }
    }

    /// <remarks/>
    public string user7
    {
        get { return this.user7Field; }
        set { this.user7Field = value; }
    }

    /// <remarks/>
    public string user8
    {
        get { return this.user8Field; }
        set { this.user8Field = value; }
    }

    /// <remarks/>
    public string user9
    {
        get { return this.user9Field; }
        set { this.user9Field = value; }
    }

    /// <remarks/>
    public string taxableFlag
    {
        get { return this.taxableFlagField; }
        set { this.taxableFlagField = value; }
    }

    /// <remarks/>
    public double floorPlanningCustomer
    {
        get { return this.floorPlanningCustomerField; }
        set { this.floorPlanningCustomerField = value; }
    }

    /// <remarks/>
    public string approvalType
    {
        get { return this.approvalTypeField; }
        set { this.approvalTypeField = value; }
    }

    /// <remarks/>
    public string lostBusinessType
    {
        get { return this.lostBusinessTypeField; }
        set { this.lostBusinessTypeField = value; }
    }

    /// <remarks/>
    public string salesRepIn
    {
        get { return this.salesRepInField; }
        set { this.salesRepInField = value; }
    }

    /// <remarks/>
    public string salesRepOut
    {
        get { return this.salesRepOutField; }
        set { this.salesRepOutField = value; }
    }

    /// <remarks/>
    public string takenBy
    {
        get { return this.takenByField; }
        set { this.takenByField = value; }
    }

    /// <remarks/>
    public double addonAmount1
    {
        get { return this.addonAmount1Field; }
        set { this.addonAmount1Field = value; }
    }

    /// <remarks/>
    public double addonAmount2
    {
        get { return this.addonAmount2Field; }
        set { this.addonAmount2Field = value; }
    }

    /// <remarks/>
    public double addonAmount3
    {
        get { return this.addonAmount3Field; }
        set { this.addonAmount3Field = value; }
    }

    /// <remarks/>
    public double addonAmount4
    {
        get { return this.addonAmount4Field; }
        set { this.addonAmount4Field = value; }
    }

    /// <remarks/>
    public int addonNumber1
    {
        get { return this.addonNumber1Field; }
        set { this.addonNumber1Field = value; }
    }

    /// <remarks/>
    public int addonNumber2
    {
        get { return this.addonNumber2Field; }
        set { this.addonNumber2Field = value; }
    }

    /// <remarks/>
    public int addonNumber3
    {
        get { return this.addonNumber3Field; }
        set { this.addonNumber3Field = value; }
    }

    /// <remarks/>
    public int addonNumber4
    {
        get { return this.addonNumber4Field; }
        set { this.addonNumber4Field = value; }
    }

    /// <remarks/>
    public string addonType1
    {
        get { return this.addonType1Field; }
        set { this.addonType1Field = value; }
    }

    /// <remarks/>
    public string addonType2
    {
        get { return this.addonType2Field; }
        set { this.addonType2Field = value; }
    }

    /// <remarks/>
    public string addonType3
    {
        get { return this.addonType3Field; }
        set { this.addonType3Field = value; }
    }

    /// <remarks/>
    public string addonType4
    {
        get { return this.addonType4Field; }
        set { this.addonType4Field = value; }
    }

    /// <remarks/>
    public int addonTaxGroup1
    {
        get { return this.addonTaxGroup1Field; }
        set { this.addonTaxGroup1Field = value; }
    }

    /// <remarks/>
    public int addonTaxGroup2
    {
        get { return this.addonTaxGroup2Field; }
        set { this.addonTaxGroup2Field = value; }
    }

    /// <remarks/>
    public int addonTaxGroup3
    {
        get { return this.addonTaxGroup3Field; }
        set { this.addonTaxGroup3Field = value; }
    }

    /// <remarks/>
    public int addonTaxGroup4
    {
        get { return this.addonTaxGroup4Field; }
        set { this.addonTaxGroup4Field = value; }
    }

    /// <remarks/>
    public NxtDate billDate
    {
        get { return this.billDateField; }
        set { this.billDateField = value; }
    }

    /// <remarks/>
    public double contactId
    {
        get { return this.contactIdField; }
        set { this.contactIdField = value; }
    }

    /// <remarks/>
    public string creditReasonType
    {
        get { return this.creditReasonTypeField; }
        set { this.creditReasonTypeField = value; }
    }

    /// <remarks/>
    public string currencyType
    {
        get { return this.currencyTypeField; }
        set { this.currencyTypeField = value; }
    }

    /// <remarks/>
    public int divisionNumber
    {
        get { return this.divisionNumberField; }
        set { this.divisionNumberField = value; }
    }

    /// <remarks/>
    public NxtDate directRouteDeliveryDate
    {
        get { return this.directRouteDeliveryDateField; }
        set { this.directRouteDeliveryDateField = value; }
    }

    /// <remarks/>
    public string directRouteDeliveryTime
    {
        get { return this.directRouteDeliveryTimeField; }
        set { this.directRouteDeliveryTimeField = value; }
    }

    /// <remarks/>
    public bool directRouteHoldFlag
    {
        get { return this.directRouteHoldFlagField; }
        set { this.directRouteHoldFlagField = value; }
    }

    /// <remarks/>
    public double downPaymentAmount
    {
        get { return this.downPaymentAmountField; }
        set { this.downPaymentAmountField = value; }
    }

    /// <remarks/>
    public int geoCode
    {
        get { return this.geoCodeField; }
        set { this.geoCodeField = value; }
    }

    /// <remarks/>
    public bool inboundFreightFlag
    {
        get { return this.inboundFreightFlagField; }
        set { this.inboundFreightFlagField = value; }
    }

    /// <remarks/>
    public string jobNumber
    {
        get { return this.jobNumberField; }
        set { this.jobNumberField = value; }
    }

    /// <remarks/>
    public string languageCode
    {
        get { return this.languageCodeField; }
        set { this.languageCodeField = value; }
    }

    /// <remarks/>
    public bool lockFlag
    {
        get { return this.lockFlagField; }
        set { this.lockFlagField = value; }
    }

    /// <remarks/>
    public int longestLeadTimeDays
    {
        get { return this.longestLeadTimeDaysField; }
        set { this.longestLeadTimeDaysField = value; }
    }

    /// <remarks/>
    public double lumpBillingAmount
    {
        get { return this.lumpBillingAmountField; }
        set { this.lumpBillingAmountField = value; }
    }

    /// <remarks/>
    public bool lumpBillingFlag
    {
        get { return this.lumpBillingFlagField; }
        set { this.lumpBillingFlagField = value; }
    }

    /// <remarks/>
    public bool lumpBillingPriceFlag
    {
        get { return this.lumpBillingPriceFlagField; }
        set { this.lumpBillingPriceFlagField = value; }
    }

    /// <remarks/>
    public string nonTaxType
    {
        get { return this.nonTaxTypeField; }
        set { this.nonTaxTypeField = value; }
    }

    /// <remarks/>
    public bool outboundFreightFlag
    {
        get { return this.outboundFreightFlagField; }
        set { this.outboundFreightFlagField = value; }
    }

    /// <remarks/>
    public double paymentAmount1
    {
        get { return this.paymentAmount1Field; }
        set { this.paymentAmount1Field = value; }
    }

    /// <remarks/>
    public double paymentAmount2
    {
        get { return this.paymentAmount2Field; }
        set { this.paymentAmount2Field = value; }
    }

    /// <remarks/>
    public double paymentAmount3
    {
        get { return this.paymentAmount3Field; }
        set { this.paymentAmount3Field = value; }
    }

    /// <remarks/>
    public bool printPricesOnPickTickets
    {
        get { return this.printPricesOnPickTicketsField; }
        set { this.printPricesOnPickTicketsField = value; }
    }

    /// <remarks/>
    public int priceCode
    {
        get { return this.priceCodeField; }
        set { this.priceCodeField = value; }
    }

    /// <remarks/>
    public bool reprintPickTicket
    {
        get { return this.reprintPickTicketField; }
        set { this.reprintPickTicketField = value; }
    }

    /// <remarks/>
    public bool printPricesFlag
    {
        get { return this.printPricesFlagField; }
        set { this.printPricesFlagField = value; }
    }

    /// <remarks/>
    public string pstLicenseNumber
    {
        get { return this.pstLicenseNumberField; }
        set { this.pstLicenseNumberField = value; }
    }

    /// <remarks/>
    public double pstTaxAmount
    {
        get { return this.pstTaxAmountField; }
        set { this.pstTaxAmountField = value; }
    }

    /// <remarks/>
    public string route
    {
        get { return this.routeField; }
        set { this.routeField = value; }
    }

    /// <remarks/>
    public string sourcePros
    {
        get { return this.sourceProsField; }
        set { this.sourceProsField = value; }
    }

    /// <remarks/>
    public double specialDiscountAmount
    {
        get { return this.specialDiscountAmountField; }
        set { this.specialDiscountAmountField = value; }
    }

    /// <remarks/>
    public string stagingArea
    {
        get { return this.stagingAreaField; }
        set { this.stagingAreaField = value; }
    }

    /// <remarks/>
    public string stateCode
    {
        get { return this.stateCodeField; }
        set { this.stateCodeField = value; }
    }

    /// <remarks/>
    public int standingOrderDays
    {
        get { return this.standingOrderDaysField; }
        set { this.standingOrderDaysField = value; }
    }

    /// <remarks/>
    public bool standingOrderType
    {
        get { return this.standingOrderTypeField; }
        set { this.standingOrderTypeField = value; }
    }

    /// <remarks/>
    public string taxAuthority
    {
        get { return this.taxAuthorityField; }
        set { this.taxAuthorityField = value; }
    }

    /// <remarks/>
    public string taxDefaultType
    {
        get { return this.taxDefaultTypeField; }
        set { this.taxDefaultTypeField = value; }
    }

    /// <remarks/>
    public double tenderAmount
    {
        get { return this.tenderAmountField; }
        set { this.tenderAmountField = value; }
    }

    /// <remarks/>
    public double termsDiscountAmount
    {
        get { return this.termsDiscountAmountField; }
        set { this.termsDiscountAmountField = value; }
    }

    /// <remarks/>
    public bool termsLineFlag
    {
        get { return this.termsLineFlagField; }
        set { this.termsLineFlagField = value; }
    }

    /// <remarks/>
    public double termsPercent
    {
        get { return this.termsPercentField; }
        set { this.termsPercentField = value; }
    }

    /// <remarks/>
    public string updateType
    {
        get { return this.updateTypeField; }
        set { this.updateTypeField = value; }
    }

    /// <remarks/>
    public string wholeOrderDefaultType
    {
        get { return this.wholeOrderDefaultTypeField; }
        set { this.wholeOrderDefaultTypeField = value; }
    }

    /// <remarks/>
    public double wholeOrderDiscountAmount
    {
        get { return this.wholeOrderDiscountAmountField; }
        set { this.wholeOrderDiscountAmountField = value; }
    }

    /// <remarks/>
    public double wholeOrderDiscountPercent
    {
        get { return this.wholeOrderDiscountPercentField; }
        set { this.wholeOrderDiscountPercentField = value; }
    }

    /// <remarks/>
    public string wholeOrderDiscountType
    {
        get { return this.wholeOrderDiscountTypeField; }
        set { this.wholeOrderDiscountTypeField = value; }
    }

    /// <remarks/>
    public double writeOffAmount
    {
        get { return this.writeOffAmountField; }
        set { this.writeOffAmountField = value; }
    }

    /// <remarks/>
    public string zone
    {
        get { return this.zoneField; }
        set { this.zoneField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class NxtDate
{
    private int yearField;

    private int monthField;

    private int dateField;

    /// <remarks/>
    public int year
    {
        get { return this.yearField; }
        set { this.yearField = value; }
    }

    /// <remarks/>
    public int month
    {
        get { return this.monthField; }
        set { this.monthField = value; }
    }

    /// <remarks/>
    public int date
    {
        get { return this.dateField; }
        set { this.dateField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.HeaderExtra",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputHeaderExtra
{
    private string fieldNameField;

    private string fieldValueField;

    private int sequenceNumberField;

    /// <remarks/>
    public string fieldName
    {
        get { return this.fieldNameField; }
        set { this.fieldNameField = value; }
    }

    /// <remarks/>
    public string fieldValue
    {
        get { return this.fieldValueField; }
        set { this.fieldValueField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.LineExtra",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputLineExtra
{
    private string lineIdentifierField;

    private int sequenceNumberField;

    private string fieldNameField;

    private string fieldValueField;

    /// <remarks/>
    public string lineIdentifier
    {
        get { return this.lineIdentifierField; }
        set { this.lineIdentifierField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public string fieldName
    {
        get { return this.fieldNameField; }
        set { this.fieldNameField = value; }
    }

    /// <remarks/>
    public string fieldValue
    {
        get { return this.fieldValueField; }
        set { this.fieldValueField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Item",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputItem
{
    private string buyerProductCodeField;

    private string descriptionField;

    private string dueDateField;

    private string expectedShipDateField;

    private string lineCommentsField;

    private string lineIdentifierField;

    private string promiseDateField;

    private string quantityOrderedField;

    private string unitOfMeasureField;

    private string requestedShipDateField;

    private string sellerProductCodeField;

    private string specialPriceUnitOfMeasureField;

    private string sxEnterpriseLineNumberField;

    private string unitCostField;

    private string upcField;

    private string user1Field;

    private string user10Field;

    private string user2Field;

    private string user3Field;

    private string user4Field;

    private string user5Field;

    private string user6Field;

    private string user7Field;

    private string user8Field;

    private string user9Field;

    private string taxableFlagField;

    private string backorderTypeField;

    private string orderTypeField;

    private int orderAlternateNumberField;

    private bool printPriceFlagField;

    private string productCategoryField;

    private string productLineField;

    private double productCostField;

    private string specialNonStockTypeField;

    private bool subtotalFlagField;

    private bool usageFlagField;

    private double vendorNumberField;

    private double discountAmountField;

    private bool discountTypeField;

    private string advertisingCodeField;

    private string alternateWarehouseField;

    private string arpProductLineField;

    private double arpVendorNumberField;

    private string binLocationField;

    private string commtypeField;

    private double coreChargeField;

    private string coreChargeTypeField;

    private string coreReturnTypeField;

    private string creditReasonTypeField;

    private int discountCodeField;

    private string jobNumberField;

    private int leadTimeField;

    private int lineAlternateNumberField;

    private string lostBusinessTypeField;

    private string nonTaxTypeField;

    private int priceDiscountingRecordNumberField;

    private double priceCodeField;

    private string priceCalcTypeField;

    private string priceCostTypeField;

    private string priceTypeField;

    private bool printPickTicketsField;

    private double quantityUnAvailableField;

    private string reasonUnAvailableTypeField;

    private string requestedProductField;

    private bool restockingFlagField;

    private bool returnFlagField;

    private string returnTypeField;

    private bool rushFlagField;

    private string salesTerritoryField;

    private string salesRepInField;

    private string salesRepOutField;

    private string subtotalDescriptionField;

    private string tariffCodeField;

    private double termsPercentField;

    private double wholeOrderDiscountAmount1Field;

    private double wholeOrderDiscountAmount2Field;

    private string crossReferenceProductTypeField;

    /// <remarks/>
    public string buyerProductCode
    {
        get { return this.buyerProductCodeField; }
        set { this.buyerProductCodeField = value; }
    }

    /// <remarks/>
    public string description
    {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public string dueDate
    {
        get { return this.dueDateField; }
        set { this.dueDateField = value; }
    }

    /// <remarks/>
    public string expectedShipDate
    {
        get { return this.expectedShipDateField; }
        set { this.expectedShipDateField = value; }
    }

    /// <remarks/>
    public string lineComments
    {
        get { return this.lineCommentsField; }
        set { this.lineCommentsField = value; }
    }

    /// <remarks/>
    public string lineIdentifier
    {
        get { return this.lineIdentifierField; }
        set { this.lineIdentifierField = value; }
    }

    /// <remarks/>
    public string promiseDate
    {
        get { return this.promiseDateField; }
        set { this.promiseDateField = value; }
    }

    /// <remarks/>
    public string quantityOrdered
    {
        get { return this.quantityOrderedField; }
        set { this.quantityOrderedField = value; }
    }

    /// <remarks/>
    public string unitOfMeasure
    {
        get { return this.unitOfMeasureField; }
        set { this.unitOfMeasureField = value; }
    }

    /// <remarks/>
    public string requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public string sellerProductCode
    {
        get { return this.sellerProductCodeField; }
        set { this.sellerProductCodeField = value; }
    }

    /// <remarks/>
    public string specialPriceUnitOfMeasure
    {
        get { return this.specialPriceUnitOfMeasureField; }
        set { this.specialPriceUnitOfMeasureField = value; }
    }

    /// <remarks/>
    public string sxEnterpriseLineNumber
    {
        get { return this.sxEnterpriseLineNumberField; }
        set { this.sxEnterpriseLineNumberField = value; }
    }

    /// <remarks/>
    public string unitCost
    {
        get { return this.unitCostField; }
        set { this.unitCostField = value; }
    }

    /// <remarks/>
    public string upc
    {
        get { return this.upcField; }
        set { this.upcField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user10
    {
        get { return this.user10Field; }
        set { this.user10Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }

    /// <remarks/>
    public string user3
    {
        get { return this.user3Field; }
        set { this.user3Field = value; }
    }

    /// <remarks/>
    public string user4
    {
        get { return this.user4Field; }
        set { this.user4Field = value; }
    }

    /// <remarks/>
    public string user5
    {
        get { return this.user5Field; }
        set { this.user5Field = value; }
    }

    /// <remarks/>
    public string user6
    {
        get { return this.user6Field; }
        set { this.user6Field = value; }
    }

    /// <remarks/>
    public string user7
    {
        get { return this.user7Field; }
        set { this.user7Field = value; }
    }

    /// <remarks/>
    public string user8
    {
        get { return this.user8Field; }
        set { this.user8Field = value; }
    }

    /// <remarks/>
    public string user9
    {
        get { return this.user9Field; }
        set { this.user9Field = value; }
    }

    /// <remarks/>
    public string taxableFlag
    {
        get { return this.taxableFlagField; }
        set { this.taxableFlagField = value; }
    }

    /// <remarks/>
    public string backorderType
    {
        get { return this.backorderTypeField; }
        set { this.backorderTypeField = value; }
    }

    /// <remarks/>
    public string orderType
    {
        get { return this.orderTypeField; }
        set { this.orderTypeField = value; }
    }

    /// <remarks/>
    public int orderAlternateNumber
    {
        get { return this.orderAlternateNumberField; }
        set { this.orderAlternateNumberField = value; }
    }

    /// <remarks/>
    public bool printPriceFlag
    {
        get { return this.printPriceFlagField; }
        set { this.printPriceFlagField = value; }
    }

    /// <remarks/>
    public string productCategory
    {
        get { return this.productCategoryField; }
        set { this.productCategoryField = value; }
    }

    /// <remarks/>
    public string productLine
    {
        get { return this.productLineField; }
        set { this.productLineField = value; }
    }

    /// <remarks/>
    public double productCost
    {
        get { return this.productCostField; }
        set { this.productCostField = value; }
    }

    /// <remarks/>
    public string specialNonStockType
    {
        get { return this.specialNonStockTypeField; }
        set { this.specialNonStockTypeField = value; }
    }

    /// <remarks/>
    public bool subtotalFlag
    {
        get { return this.subtotalFlagField; }
        set { this.subtotalFlagField = value; }
    }

    /// <remarks/>
    public bool usageFlag
    {
        get { return this.usageFlagField; }
        set { this.usageFlagField = value; }
    }

    /// <remarks/>
    public double vendorNumber
    {
        get { return this.vendorNumberField; }
        set { this.vendorNumberField = value; }
    }

    /// <remarks/>
    public double discountAmount
    {
        get { return this.discountAmountField; }
        set { this.discountAmountField = value; }
    }

    /// <remarks/>
    public bool discountType
    {
        get { return this.discountTypeField; }
        set { this.discountTypeField = value; }
    }

    /// <remarks/>
    public string advertisingCode
    {
        get { return this.advertisingCodeField; }
        set { this.advertisingCodeField = value; }
    }

    /// <remarks/>
    public string alternateWarehouse
    {
        get { return this.alternateWarehouseField; }
        set { this.alternateWarehouseField = value; }
    }

    /// <remarks/>
    public string arpProductLine
    {
        get { return this.arpProductLineField; }
        set { this.arpProductLineField = value; }
    }

    /// <remarks/>
    public double arpVendorNumber
    {
        get { return this.arpVendorNumberField; }
        set { this.arpVendorNumberField = value; }
    }

    /// <remarks/>
    public string binLocation
    {
        get { return this.binLocationField; }
        set { this.binLocationField = value; }
    }

    /// <remarks/>
    public string commtype
    {
        get { return this.commtypeField; }
        set { this.commtypeField = value; }
    }

    /// <remarks/>
    public double coreCharge
    {
        get { return this.coreChargeField; }
        set { this.coreChargeField = value; }
    }

    /// <remarks/>
    public string coreChargeType
    {
        get { return this.coreChargeTypeField; }
        set { this.coreChargeTypeField = value; }
    }

    /// <remarks/>
    public string coreReturnType
    {
        get { return this.coreReturnTypeField; }
        set { this.coreReturnTypeField = value; }
    }

    /// <remarks/>
    public string creditReasonType
    {
        get { return this.creditReasonTypeField; }
        set { this.creditReasonTypeField = value; }
    }

    /// <remarks/>
    public int discountCode
    {
        get { return this.discountCodeField; }
        set { this.discountCodeField = value; }
    }

    /// <remarks/>
    public string jobNumber
    {
        get { return this.jobNumberField; }
        set { this.jobNumberField = value; }
    }

    /// <remarks/>
    public int leadTime
    {
        get { return this.leadTimeField; }
        set { this.leadTimeField = value; }
    }

    /// <remarks/>
    public int lineAlternateNumber
    {
        get { return this.lineAlternateNumberField; }
        set { this.lineAlternateNumberField = value; }
    }

    /// <remarks/>
    public string lostBusinessType
    {
        get { return this.lostBusinessTypeField; }
        set { this.lostBusinessTypeField = value; }
    }

    /// <remarks/>
    public string nonTaxType
    {
        get { return this.nonTaxTypeField; }
        set { this.nonTaxTypeField = value; }
    }

    /// <remarks/>
    public int priceDiscountingRecordNumber
    {
        get { return this.priceDiscountingRecordNumberField; }
        set { this.priceDiscountingRecordNumberField = value; }
    }

    /// <remarks/>
    public double priceCode
    {
        get { return this.priceCodeField; }
        set { this.priceCodeField = value; }
    }

    /// <remarks/>
    public string priceCalcType
    {
        get { return this.priceCalcTypeField; }
        set { this.priceCalcTypeField = value; }
    }

    /// <remarks/>
    public string priceCostType
    {
        get { return this.priceCostTypeField; }
        set { this.priceCostTypeField = value; }
    }

    /// <remarks/>
    public string priceType
    {
        get { return this.priceTypeField; }
        set { this.priceTypeField = value; }
    }

    /// <remarks/>
    public bool printPickTickets
    {
        get { return this.printPickTicketsField; }
        set { this.printPickTicketsField = value; }
    }

    /// <remarks/>
    public double quantityUnAvailable
    {
        get { return this.quantityUnAvailableField; }
        set { this.quantityUnAvailableField = value; }
    }

    /// <remarks/>
    public string reasonUnAvailableType
    {
        get { return this.reasonUnAvailableTypeField; }
        set { this.reasonUnAvailableTypeField = value; }
    }

    /// <remarks/>
    public string requestedProduct
    {
        get { return this.requestedProductField; }
        set { this.requestedProductField = value; }
    }

    /// <remarks/>
    public bool restockingFlag
    {
        get { return this.restockingFlagField; }
        set { this.restockingFlagField = value; }
    }

    /// <remarks/>
    public bool returnFlag
    {
        get { return this.returnFlagField; }
        set { this.returnFlagField = value; }
    }

    /// <remarks/>
    public string returnType
    {
        get { return this.returnTypeField; }
        set { this.returnTypeField = value; }
    }

    /// <remarks/>
    public bool rushFlag
    {
        get { return this.rushFlagField; }
        set { this.rushFlagField = value; }
    }

    /// <remarks/>
    public string salesTerritory
    {
        get { return this.salesTerritoryField; }
        set { this.salesTerritoryField = value; }
    }

    /// <remarks/>
    public string salesRepIn
    {
        get { return this.salesRepInField; }
        set { this.salesRepInField = value; }
    }

    /// <remarks/>
    public string salesRepOut
    {
        get { return this.salesRepOutField; }
        set { this.salesRepOutField = value; }
    }

    /// <remarks/>
    public string subtotalDescription
    {
        get { return this.subtotalDescriptionField; }
        set { this.subtotalDescriptionField = value; }
    }

    /// <remarks/>
    public string tariffCode
    {
        get { return this.tariffCodeField; }
        set { this.tariffCodeField = value; }
    }

    /// <remarks/>
    public double termsPercent
    {
        get { return this.termsPercentField; }
        set { this.termsPercentField = value; }
    }

    /// <remarks/>
    public double wholeOrderDiscountAmount1
    {
        get { return this.wholeOrderDiscountAmount1Field; }
        set { this.wholeOrderDiscountAmount1Field = value; }
    }

    /// <remarks/>
    public double wholeOrderDiscountAmount2
    {
        get { return this.wholeOrderDiscountAmount2Field; }
        set { this.wholeOrderDiscountAmount2Field = value; }
    }

    /// <remarks/>
    public string crossReferenceProductType
    {
        get { return this.crossReferenceProductTypeField; }
        set { this.crossReferenceProductTypeField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Schedule",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputSchedule
{
    private string lineIdentifierField;

    private string scheduleDateField;

    private string scheduleCodeField;

    private int sequenceNumberField;

    private string shipToAddress1Field;

    private string shipToAddress2Field;

    private string shipToCityField;

    private string shipToContactField;

    private string shipToCountryCodeField;

    private string shipToDunsNumberField;

    private string shipToNumberField;

    private string shipToPhoneField;

    private string shipToPostalCodeField;

    private string shipToStateField;

    private string shipViaField;

    private string sxEnterpriseLineNumberField;

    private string unitOfMeasureField;

    /// <remarks/>
    public string lineIdentifier
    {
        get { return this.lineIdentifierField; }
        set { this.lineIdentifierField = value; }
    }

    /// <remarks/>
    public string scheduleDate
    {
        get { return this.scheduleDateField; }
        set { this.scheduleDateField = value; }
    }

    /// <remarks/>
    public string scheduleCode
    {
        get { return this.scheduleCodeField; }
        set { this.scheduleCodeField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public string shipToAddress1
    {
        get { return this.shipToAddress1Field; }
        set { this.shipToAddress1Field = value; }
    }

    /// <remarks/>
    public string shipToAddress2
    {
        get { return this.shipToAddress2Field; }
        set { this.shipToAddress2Field = value; }
    }

    /// <remarks/>
    public string shipToCity
    {
        get { return this.shipToCityField; }
        set { this.shipToCityField = value; }
    }

    /// <remarks/>
    public string shipToContact
    {
        get { return this.shipToContactField; }
        set { this.shipToContactField = value; }
    }

    /// <remarks/>
    public string shipToCountryCode
    {
        get { return this.shipToCountryCodeField; }
        set { this.shipToCountryCodeField = value; }
    }

    /// <remarks/>
    public string shipToDunsNumber
    {
        get { return this.shipToDunsNumberField; }
        set { this.shipToDunsNumberField = value; }
    }

    /// <remarks/>
    public string shipToNumber
    {
        get { return this.shipToNumberField; }
        set { this.shipToNumberField = value; }
    }

    /// <remarks/>
    public string shipToPhone
    {
        get { return this.shipToPhoneField; }
        set { this.shipToPhoneField = value; }
    }

    /// <remarks/>
    public string shipToPostalCode
    {
        get { return this.shipToPostalCodeField; }
        set { this.shipToPostalCodeField = value; }
    }

    /// <remarks/>
    public string shipToState
    {
        get { return this.shipToStateField; }
        set { this.shipToStateField = value; }
    }

    /// <remarks/>
    public string shipVia
    {
        get { return this.shipViaField; }
        set { this.shipViaField = value; }
    }

    /// <remarks/>
    public string sxEnterpriseLineNumber
    {
        get { return this.sxEnterpriseLineNumberField; }
        set { this.sxEnterpriseLineNumberField = value; }
    }

    /// <remarks/>
    public string unitOfMeasure
    {
        get { return this.unitOfMeasureField; }
        set { this.unitOfMeasureField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.ShipFrom",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputShipFrom
{
    private string address1Field;

    private string address2Field;

    private string cityField;

    private string contactField;

    private string countryCodeField;

    private string dunsNumberField;

    private string nameField;

    private string phoneField;

    private string postalCodeField;

    private string shipFromField;

    private string stateField;

    private string user1Field;

    private string user2Field;

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string contact
    {
        get { return this.contactField; }
        set { this.contactField = value; }
    }

    /// <remarks/>
    public string countryCode
    {
        get { return this.countryCodeField; }
        set { this.countryCodeField = value; }
    }

    /// <remarks/>
    public string dunsNumber
    {
        get { return this.dunsNumberField; }
        set { this.dunsNumberField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string phone
    {
        get { return this.phoneField; }
        set { this.phoneField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public string shipFrom
    {
        get { return this.shipFromField; }
        set { this.shipFromField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.ShipTo",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputShipTo
{
    private string address1Field;

    private string address2Field;

    private string cityField;

    private string contactField;

    private string countryCodeField;

    private string dunsNumberField;

    private string nameField;

    private string phoneField;

    private string postalCodeField;

    private string shipToNumberField;

    private string stateField;

    private string user1Field;

    private string user2Field;

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string contact
    {
        get { return this.contactField; }
        set { this.contactField = value; }
    }

    /// <remarks/>
    public string countryCode
    {
        get { return this.countryCodeField; }
        set { this.countryCodeField = value; }
    }

    /// <remarks/>
    public string dunsNumber
    {
        get { return this.dunsNumberField; }
        set { this.dunsNumberField = value; }
    }

    /// <remarks/>
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }

    /// <remarks/>
    public string phone
    {
        get { return this.phoneField; }
        set { this.phoneField = value; }
    }

    /// <remarks/>
    public string postalCode
    {
        get { return this.postalCodeField; }
        set { this.postalCodeField = value; }
    }

    /// <remarks/>
    public string shipToNumber
    {
        get { return this.shipToNumberField; }
        set { this.shipToNumberField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Total",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputTotal
{
    private string invoiceAmountField;

    private string numberOfLinesField;

    private string quantityOrderedField;

    /// <remarks/>
    public string invoiceAmount
    {
        get { return this.invoiceAmountField; }
        set { this.invoiceAmountField = value; }
    }

    /// <remarks/>
    public string numberOfLines
    {
        get { return this.numberOfLinesField; }
        set { this.numberOfLinesField = value; }
    }

    /// <remarks/>
    public string quantityOrdered
    {
        get { return this.quantityOrderedField; }
        set { this.quantityOrderedField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.input.Terms",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5inputTerms
{
    private string basisDateCodeField;

    private string cardAccountNumberField;

    private string cardExpirationDateField;

    private string cardPreAuthorizationDateField;

    private string cardPreAuthorizationNumberField;

    private string cardTypeField;

    private string descriptionField;

    private string discountDaysField;

    private string discountDateField;

    private string discountPercentField;

    private string discountProxDayField;

    private string dueDaysField;

    private string dueDateField;

    private string sxEnterpriseTermsCodeField;

    private string typeCodeField;

    /// <remarks/>
    public string basisDateCode
    {
        get { return this.basisDateCodeField; }
        set { this.basisDateCodeField = value; }
    }

    /// <remarks/>
    public string cardAccountNumber
    {
        get { return this.cardAccountNumberField; }
        set { this.cardAccountNumberField = value; }
    }

    /// <remarks/>
    public string cardExpirationDate
    {
        get { return this.cardExpirationDateField; }
        set { this.cardExpirationDateField = value; }
    }

    /// <remarks/>
    public string cardPreAuthorizationDate
    {
        get { return this.cardPreAuthorizationDateField; }
        set { this.cardPreAuthorizationDateField = value; }
    }

    /// <remarks/>
    public string cardPreAuthorizationNumber
    {
        get { return this.cardPreAuthorizationNumberField; }
        set { this.cardPreAuthorizationNumberField = value; }
    }

    /// <remarks/>
    public string cardType
    {
        get { return this.cardTypeField; }
        set { this.cardTypeField = value; }
    }

    /// <remarks/>
    public string description
    {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public string discountDays
    {
        get { return this.discountDaysField; }
        set { this.discountDaysField = value; }
    }

    /// <remarks/>
    public string discountDate
    {
        get { return this.discountDateField; }
        set { this.discountDateField = value; }
    }

    /// <remarks/>
    public string discountPercent
    {
        get { return this.discountPercentField; }
        set { this.discountPercentField = value; }
    }

    /// <remarks/>
    public string discountProxDay
    {
        get { return this.discountProxDayField; }
        set { this.discountProxDayField = value; }
    }

    /// <remarks/>
    public string dueDays
    {
        get { return this.dueDaysField; }
        set { this.dueDaysField = value; }
    }

    /// <remarks/>
    public string dueDate
    {
        get { return this.dueDateField; }
        set { this.dueDateField = value; }
    }

    /// <remarks/>
    public string sxEnterpriseTermsCode
    {
        get { return this.sxEnterpriseTermsCodeField; }
        set { this.sxEnterpriseTermsCodeField = value; }
    }

    /// <remarks/>
    public string typeCode
    {
        get { return this.typeCodeField; }
        set { this.typeCodeField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.output.Acknowledgement",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5outputAcknowledgement
{
    private int companyNumberField;

    private string correlationDataField;

    private string data1Field;

    private int errorNumberField;

    private string messageField;

    private int sequenceNumberField;

    private string transactionTypeField;

    /// <remarks/>
    public int companyNumber
    {
        get { return this.companyNumberField; }
        set { this.companyNumberField = value; }
    }

    /// <remarks/>
    public string correlationData
    {
        get { return this.correlationDataField; }
        set { this.correlationDataField = value; }
    }

    /// <remarks/>
    public string data1
    {
        get { return this.data1Field; }
        set { this.data1Field = value; }
    }

    /// <remarks/>
    public int errorNumber
    {
        get { return this.errorNumberField; }
        set { this.errorNumberField = value; }
    }

    /// <remarks/>
    public string message
    {
        get { return this.messageField; }
        set { this.messageField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public string transactionType
    {
        get { return this.transactionTypeField; }
        set { this.transactionTypeField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.output.Header",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5outputHeader
{
    private string invoiceDateField;

    private string invoiceNumberField;

    private string invoiceSuffixField;

    private string customerPurchaseOrderField;

    private string invoiceTypeField;

    private string referenceField;

    private string partnerIdField;

    private string buyingPartyField;

    private string departmentField;

    private string orderDispositionField;

    private string eventField;

    private string vendorNumber2Field;

    private string cancelDateField;

    private string shipDateField;

    private string promiseDateField;

    private string requestedShipDateField;

    private string shipViaField;

    private string purchaseOrderIssueDateField;

    private string enterDateField;

    private string packageIdField;

    private string acknowledgementTypeField;

    private string currentDateField;

    private string user1Field;

    private string user2Field;

    private string user3Field;

    private string userInvoiceField;

    private string transactionTypeField;

    private string shippingInstructionsField;

    private string placedByField;

    private string warehouseField;

    private string coreChargeField;

    private string datcCostField;

    private string downPaymentField;

    private string specialDiscountAmountField;

    private string restockAmountField;

    private string taxAmountField;

    private string gstTaxAmountField;

    private string pstTaxAmountField;

    private string workOrderDiscountAmountField;

    private string termsDiscountAmountField;

    private string companyNumberField;

    /// <remarks/>
    public string invoiceDate
    {
        get { return this.invoiceDateField; }
        set { this.invoiceDateField = value; }
    }

    /// <remarks/>
    public string invoiceNumber
    {
        get { return this.invoiceNumberField; }
        set { this.invoiceNumberField = value; }
    }

    /// <remarks/>
    public string invoiceSuffix
    {
        get { return this.invoiceSuffixField; }
        set { this.invoiceSuffixField = value; }
    }

    /// <remarks/>
    public string customerPurchaseOrder
    {
        get { return this.customerPurchaseOrderField; }
        set { this.customerPurchaseOrderField = value; }
    }

    /// <remarks/>
    public string invoiceType
    {
        get { return this.invoiceTypeField; }
        set { this.invoiceTypeField = value; }
    }

    /// <remarks/>
    public string reference
    {
        get { return this.referenceField; }
        set { this.referenceField = value; }
    }

    /// <remarks/>
    public string partnerId
    {
        get { return this.partnerIdField; }
        set { this.partnerIdField = value; }
    }

    /// <remarks/>
    public string buyingParty
    {
        get { return this.buyingPartyField; }
        set { this.buyingPartyField = value; }
    }

    /// <remarks/>
    public string department
    {
        get { return this.departmentField; }
        set { this.departmentField = value; }
    }

    /// <remarks/>
    public string orderDisposition
    {
        get { return this.orderDispositionField; }
        set { this.orderDispositionField = value; }
    }

    /// <remarks/>
    public string @event
    {
        get { return this.eventField; }
        set { this.eventField = value; }
    }

    /// <remarks/>
    public string vendorNumber2
    {
        get { return this.vendorNumber2Field; }
        set { this.vendorNumber2Field = value; }
    }

    /// <remarks/>
    public string cancelDate
    {
        get { return this.cancelDateField; }
        set { this.cancelDateField = value; }
    }

    /// <remarks/>
    public string shipDate
    {
        get { return this.shipDateField; }
        set { this.shipDateField = value; }
    }

    /// <remarks/>
    public string promiseDate
    {
        get { return this.promiseDateField; }
        set { this.promiseDateField = value; }
    }

    /// <remarks/>
    public string requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public string shipVia
    {
        get { return this.shipViaField; }
        set { this.shipViaField = value; }
    }

    /// <remarks/>
    public string purchaseOrderIssueDate
    {
        get { return this.purchaseOrderIssueDateField; }
        set { this.purchaseOrderIssueDateField = value; }
    }

    /// <remarks/>
    public string enterDate
    {
        get { return this.enterDateField; }
        set { this.enterDateField = value; }
    }

    /// <remarks/>
    public string packageId
    {
        get { return this.packageIdField; }
        set { this.packageIdField = value; }
    }

    /// <remarks/>
    public string acknowledgementType
    {
        get { return this.acknowledgementTypeField; }
        set { this.acknowledgementTypeField = value; }
    }

    /// <remarks/>
    public string currentDate
    {
        get { return this.currentDateField; }
        set { this.currentDateField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }

    /// <remarks/>
    public string user3
    {
        get { return this.user3Field; }
        set { this.user3Field = value; }
    }

    /// <remarks/>
    public string userInvoice
    {
        get { return this.userInvoiceField; }
        set { this.userInvoiceField = value; }
    }

    /// <remarks/>
    public string transactionType
    {
        get { return this.transactionTypeField; }
        set { this.transactionTypeField = value; }
    }

    /// <remarks/>
    public string shippingInstructions
    {
        get { return this.shippingInstructionsField; }
        set { this.shippingInstructionsField = value; }
    }

    /// <remarks/>
    public string placedBy
    {
        get { return this.placedByField; }
        set { this.placedByField = value; }
    }

    /// <remarks/>
    public string warehouse
    {
        get { return this.warehouseField; }
        set { this.warehouseField = value; }
    }

    /// <remarks/>
    public string coreCharge
    {
        get { return this.coreChargeField; }
        set { this.coreChargeField = value; }
    }

    /// <remarks/>
    public string datcCost
    {
        get { return this.datcCostField; }
        set { this.datcCostField = value; }
    }

    /// <remarks/>
    public string downPayment
    {
        get { return this.downPaymentField; }
        set { this.downPaymentField = value; }
    }

    /// <remarks/>
    public string specialDiscountAmount
    {
        get { return this.specialDiscountAmountField; }
        set { this.specialDiscountAmountField = value; }
    }

    /// <remarks/>
    public string restockAmount
    {
        get { return this.restockAmountField; }
        set { this.restockAmountField = value; }
    }

    /// <remarks/>
    public string taxAmount
    {
        get { return this.taxAmountField; }
        set { this.taxAmountField = value; }
    }

    /// <remarks/>
    public string gstTaxAmount
    {
        get { return this.gstTaxAmountField; }
        set { this.gstTaxAmountField = value; }
    }

    /// <remarks/>
    public string pstTaxAmount
    {
        get { return this.pstTaxAmountField; }
        set { this.pstTaxAmountField = value; }
    }

    /// <remarks/>
    public string workOrderDiscountAmount
    {
        get { return this.workOrderDiscountAmountField; }
        set { this.workOrderDiscountAmountField = value; }
    }

    /// <remarks/>
    public string termsDiscountAmount
    {
        get { return this.termsDiscountAmountField; }
        set { this.termsDiscountAmountField = value; }
    }

    /// <remarks/>
    public string companyNumber
    {
        get { return this.companyNumberField; }
        set { this.companyNumberField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEFullOrderMntV5.output.Item",
    Namespace = "Nxtrend.WS"
)]
public partial class OEFullOrderMntV5outputItem
{
    private string lineIdentifierField;

    private string quantityUnitOfMeasureField;

    private string productServiceCodeField;

    private string sellerProductCodeField;

    private string buyerProductCodeField;

    private string descriptionField;

    private string user1Field;

    private string user2Field;

    private string user3Field;

    private string user4Field;

    private string user5Field;

    private string orderStatusCodeField;

    private string changeCodeField;

    private string backOrderTypeField;

    private string user6Field;

    private string user7Field;

    private string user8Field;

    private string user9Field;

    private string user10Field;

    private string specialCostTypeField;

    private string specialCostUnitOfMeasureField;

    private string crossReferenceProductTypeField;

    private string taxableFlagField;

    private string taxableTypeField;

    private string taxGroupField;

    private string promiseDateField;

    private string requestedShipDateField;

    private string specialNonStockTypeField;

    private string upcField;

    private string sxEnterpriseLineNumberField;

    private string quantityShipField;

    private string priceField;

    private string discountPercentField;

    private string quantityOrderedField;

    private string discountAmountField;

    private string taxAmount1Field;

    private string taxAmount2Field;

    private string taxAmount3Field;

    private string taxAmount4Field;

    private string upcSection1Field;

    private string upcSection2Field;

    private string upcSection3Field;

    private string upcSection4Field;

    private string upcSection5Field;

    private string upcSection6Field;

    private string restockChargeField;

    private string specialCostUnitOfMeasure2Field;

    /// <remarks/>
    public string lineIdentifier
    {
        get { return this.lineIdentifierField; }
        set { this.lineIdentifierField = value; }
    }

    /// <remarks/>
    public string quantityUnitOfMeasure
    {
        get { return this.quantityUnitOfMeasureField; }
        set { this.quantityUnitOfMeasureField = value; }
    }

    /// <remarks/>
    public string productServiceCode
    {
        get { return this.productServiceCodeField; }
        set { this.productServiceCodeField = value; }
    }

    /// <remarks/>
    public string sellerProductCode
    {
        get { return this.sellerProductCodeField; }
        set { this.sellerProductCodeField = value; }
    }

    /// <remarks/>
    public string buyerProductCode
    {
        get { return this.buyerProductCodeField; }
        set { this.buyerProductCodeField = value; }
    }

    /// <remarks/>
    public string description
    {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public string user1
    {
        get { return this.user1Field; }
        set { this.user1Field = value; }
    }

    /// <remarks/>
    public string user2
    {
        get { return this.user2Field; }
        set { this.user2Field = value; }
    }

    /// <remarks/>
    public string user3
    {
        get { return this.user3Field; }
        set { this.user3Field = value; }
    }

    /// <remarks/>
    public string user4
    {
        get { return this.user4Field; }
        set { this.user4Field = value; }
    }

    /// <remarks/>
    public string user5
    {
        get { return this.user5Field; }
        set { this.user5Field = value; }
    }

    /// <remarks/>
    public string orderStatusCode
    {
        get { return this.orderStatusCodeField; }
        set { this.orderStatusCodeField = value; }
    }

    /// <remarks/>
    public string changeCode
    {
        get { return this.changeCodeField; }
        set { this.changeCodeField = value; }
    }

    /// <remarks/>
    public string backOrderType
    {
        get { return this.backOrderTypeField; }
        set { this.backOrderTypeField = value; }
    }

    /// <remarks/>
    public string user6
    {
        get { return this.user6Field; }
        set { this.user6Field = value; }
    }

    /// <remarks/>
    public string user7
    {
        get { return this.user7Field; }
        set { this.user7Field = value; }
    }

    /// <remarks/>
    public string user8
    {
        get { return this.user8Field; }
        set { this.user8Field = value; }
    }

    /// <remarks/>
    public string user9
    {
        get { return this.user9Field; }
        set { this.user9Field = value; }
    }

    /// <remarks/>
    public string user10
    {
        get { return this.user10Field; }
        set { this.user10Field = value; }
    }

    /// <remarks/>
    public string specialCostType
    {
        get { return this.specialCostTypeField; }
        set { this.specialCostTypeField = value; }
    }

    /// <remarks/>
    public string specialCostUnitOfMeasure
    {
        get { return this.specialCostUnitOfMeasureField; }
        set { this.specialCostUnitOfMeasureField = value; }
    }

    /// <remarks/>
    public string crossReferenceProductType
    {
        get { return this.crossReferenceProductTypeField; }
        set { this.crossReferenceProductTypeField = value; }
    }

    /// <remarks/>
    public string taxableFlag
    {
        get { return this.taxableFlagField; }
        set { this.taxableFlagField = value; }
    }

    /// <remarks/>
    public string taxableType
    {
        get { return this.taxableTypeField; }
        set { this.taxableTypeField = value; }
    }

    /// <remarks/>
    public string taxGroup
    {
        get { return this.taxGroupField; }
        set { this.taxGroupField = value; }
    }

    /// <remarks/>
    public string promiseDate
    {
        get { return this.promiseDateField; }
        set { this.promiseDateField = value; }
    }

    /// <remarks/>
    public string requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public string specialNonStockType
    {
        get { return this.specialNonStockTypeField; }
        set { this.specialNonStockTypeField = value; }
    }

    /// <remarks/>
    public string upc
    {
        get { return this.upcField; }
        set { this.upcField = value; }
    }

    /// <remarks/>
    public string sxEnterpriseLineNumber
    {
        get { return this.sxEnterpriseLineNumberField; }
        set { this.sxEnterpriseLineNumberField = value; }
    }

    /// <remarks/>
    public string quantityShip
    {
        get { return this.quantityShipField; }
        set { this.quantityShipField = value; }
    }

    /// <remarks/>
    public string price
    {
        get { return this.priceField; }
        set { this.priceField = value; }
    }

    /// <remarks/>
    public string discountPercent
    {
        get { return this.discountPercentField; }
        set { this.discountPercentField = value; }
    }

    /// <remarks/>
    public string quantityOrdered
    {
        get { return this.quantityOrderedField; }
        set { this.quantityOrderedField = value; }
    }

    /// <remarks/>
    public string discountAmount
    {
        get { return this.discountAmountField; }
        set { this.discountAmountField = value; }
    }

    /// <remarks/>
    public string taxAmount1
    {
        get { return this.taxAmount1Field; }
        set { this.taxAmount1Field = value; }
    }

    /// <remarks/>
    public string taxAmount2
    {
        get { return this.taxAmount2Field; }
        set { this.taxAmount2Field = value; }
    }

    /// <remarks/>
    public string taxAmount3
    {
        get { return this.taxAmount3Field; }
        set { this.taxAmount3Field = value; }
    }

    /// <remarks/>
    public string taxAmount4
    {
        get { return this.taxAmount4Field; }
        set { this.taxAmount4Field = value; }
    }

    /// <remarks/>
    public string upcSection1
    {
        get { return this.upcSection1Field; }
        set { this.upcSection1Field = value; }
    }

    /// <remarks/>
    public string upcSection2
    {
        get { return this.upcSection2Field; }
        set { this.upcSection2Field = value; }
    }

    /// <remarks/>
    public string upcSection3
    {
        get { return this.upcSection3Field; }
        set { this.upcSection3Field = value; }
    }

    /// <remarks/>
    public string upcSection4
    {
        get { return this.upcSection4Field; }
        set { this.upcSection4Field = value; }
    }

    /// <remarks/>
    public string upcSection5
    {
        get { return this.upcSection5Field; }
        set { this.upcSection5Field = value; }
    }

    /// <remarks/>
    public string upcSection6
    {
        get { return this.upcSection6Field; }
        set { this.upcSection6Field = value; }
    }

    /// <remarks/>
    public string restockCharge
    {
        get { return this.restockChargeField; }
        set { this.restockChargeField = value; }
    }

    /// <remarks/>
    public string specialCostUnitOfMeasure2
    {
        get { return this.specialCostUnitOfMeasure2Field; }
        set { this.specialCostUnitOfMeasure2Field = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class OEGetListOfOrdersRequest
{
    private double customerNumberField;

    private string shipToField;

    private string warehouseField;

    private string transactionTypeField;

    private string takenByField;

    private string customerPurchaseOrderField;

    private int startStageField;

    private int endStageField;

    private NxtDate startEnterDateField;

    private NxtDate endEnterDateField;

    private string sort1Field;

    private string sort2Field;

    private int recordLimitField;

    /// <remarks/>
    public double customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string shipTo
    {
        get { return this.shipToField; }
        set { this.shipToField = value; }
    }

    /// <remarks/>
    public string warehouse
    {
        get { return this.warehouseField; }
        set { this.warehouseField = value; }
    }

    /// <remarks/>
    public string transactionType
    {
        get { return this.transactionTypeField; }
        set { this.transactionTypeField = value; }
    }

    /// <remarks/>
    public string takenBy
    {
        get { return this.takenByField; }
        set { this.takenByField = value; }
    }

    /// <remarks/>
    public string customerPurchaseOrder
    {
        get { return this.customerPurchaseOrderField; }
        set { this.customerPurchaseOrderField = value; }
    }

    /// <remarks/>
    public int startStage
    {
        get { return this.startStageField; }
        set { this.startStageField = value; }
    }

    /// <remarks/>
    public int endStage
    {
        get { return this.endStageField; }
        set { this.endStageField = value; }
    }

    /// <remarks/>
    public NxtDate startEnterDate
    {
        get { return this.startEnterDateField; }
        set { this.startEnterDateField = value; }
    }

    /// <remarks/>
    public NxtDate endEnterDate
    {
        get { return this.endEnterDateField; }
        set { this.endEnterDateField = value; }
    }

    /// <remarks/>
    public string sort1
    {
        get { return this.sort1Field; }
        set { this.sort1Field = value; }
    }

    /// <remarks/>
    public string sort2
    {
        get { return this.sort2Field; }
        set { this.sort2Field = value; }
    }

    /// <remarks/>
    public int recordLimit
    {
        get { return this.recordLimitField; }
        set { this.recordLimitField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class OEGetListOfOrdersResponse
{
    private string errorMessageField;

    private bool moreRecordsAvailableField;

    private OEGetListOfOrdersoutputOrder[] arrayOrderField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public bool moreRecordsAvailable
    {
        get { return this.moreRecordsAvailableField; }
        set { this.moreRecordsAvailableField = value; }
    }

    /// <remarks/>
    public OEGetListOfOrdersoutputOrder[] arrayOrder
    {
        get { return this.arrayOrderField; }
        set { this.arrayOrderField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class SAGetShipViaListRequest
{
    private string sortField;

    /// <remarks/>
    public string sort
    {
        get { return this.sortField; }
        set { this.sortField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "OEGetListOfOrders.output.Order",
    Namespace = "Nxtrend.WS"
)]
public partial class OEGetListOfOrdersoutputOrder
{
    private int orderNumberField;

    private int orderSuffixField;

    private string warehouseField;

    private double customerNumberField;

    private string shipToField;

    private int stageCodeField;

    private string stageCodeStringField;

    private NxtDate enterDateField;

    private double totalInvoiceAmountField;

    private string transactionTypeField;

    private string customerPurchaseOrderField;

    private NxtDate requestedShipDateField;

    private NxtDate promiseDateField;

    private NxtDate pickedDateField;

    private NxtDate shipDateField;

    private NxtDate invoiceDateField;

    private string sortFieldField;

    /// <remarks/>
    public int orderNumber
    {
        get { return this.orderNumberField; }
        set { this.orderNumberField = value; }
    }

    /// <remarks/>
    public int orderSuffix
    {
        get { return this.orderSuffixField; }
        set { this.orderSuffixField = value; }
    }

    /// <remarks/>
    public string warehouse
    {
        get { return this.warehouseField; }
        set { this.warehouseField = value; }
    }

    /// <remarks/>
    public double customerNumber
    {
        get { return this.customerNumberField; }
        set { this.customerNumberField = value; }
    }

    /// <remarks/>
    public string shipTo
    {
        get { return this.shipToField; }
        set { this.shipToField = value; }
    }

    /// <remarks/>
    public int stageCode
    {
        get { return this.stageCodeField; }
        set { this.stageCodeField = value; }
    }

    /// <remarks/>
    public string stageCodeString
    {
        get { return this.stageCodeStringField; }
        set { this.stageCodeStringField = value; }
    }

    /// <remarks/>
    public NxtDate enterDate
    {
        get { return this.enterDateField; }
        set { this.enterDateField = value; }
    }

    /// <remarks/>
    public double totalInvoiceAmount
    {
        get { return this.totalInvoiceAmountField; }
        set { this.totalInvoiceAmountField = value; }
    }

    /// <remarks/>
    public string transactionType
    {
        get { return this.transactionTypeField; }
        set { this.transactionTypeField = value; }
    }

    /// <remarks/>
    public string customerPurchaseOrder
    {
        get { return this.customerPurchaseOrderField; }
        set { this.customerPurchaseOrderField = value; }
    }

    /// <remarks/>
    public NxtDate requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public NxtDate promiseDate
    {
        get { return this.promiseDateField; }
        set { this.promiseDateField = value; }
    }

    /// <remarks/>
    public NxtDate pickedDate
    {
        get { return this.pickedDateField; }
        set { this.pickedDateField = value; }
    }

    /// <remarks/>
    public NxtDate shipDate
    {
        get { return this.shipDateField; }
        set { this.shipDateField = value; }
    }

    /// <remarks/>
    public NxtDate invoiceDate
    {
        get { return this.invoiceDateField; }
        set { this.invoiceDateField = value; }
    }

    /// <remarks/>
    public string sortField
    {
        get { return this.sortFieldField; }
        set { this.sortFieldField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class SAGetShipViaListResponse
{
    private string errorMessageField;

    private SAGetShipViaListoutputShipVia[] arrayShipViaField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public SAGetShipViaListoutputShipVia[] arrayShipVia
    {
        get { return this.arrayShipViaField; }
        set { this.arrayShipViaField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SAGetShipViaList.output.ShipVia",
    Namespace = "Nxtrend.WS"
)]
public partial class SAGetShipViaListoutputShipVia
{
    private string codeField;

    private string descriptionField;

    private string extraDataField;

    private string sortFieldField;

    /// <remarks/>
    public string code
    {
        get { return this.codeField; }
        set { this.codeField = value; }
    }

    /// <remarks/>
    public string description
    {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public string extraData
    {
        get { return this.extraDataField; }
        set { this.extraDataField = value; }
    }

    /// <remarks/>
    public string sortField
    {
        get { return this.sortFieldField; }
        set { this.sortFieldField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class SFOEOrderTotLoadRequest
{
    private SFOEOrderTotLoadinputCreditcard[] arrayCreditcardField;

    private SFOEOrderTotLoadinputInheader[] arrayInheaderField;

    private SFOEOrderTotLoadinputInline[] arrayInlineField;

    /// <remarks/>
    public SFOEOrderTotLoadinputCreditcard[] arrayCreditcard
    {
        get { return this.arrayCreditcardField; }
        set { this.arrayCreditcardField = value; }
    }

    /// <remarks/>
    public SFOEOrderTotLoadinputInheader[] arrayInheader
    {
        get { return this.arrayInheaderField; }
        set { this.arrayInheaderField = value; }
    }

    /// <remarks/>
    public SFOEOrderTotLoadinputInline[] arrayInline
    {
        get { return this.arrayInlineField; }
        set { this.arrayInlineField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public partial class SFOEOrderTotLoadResponse
{
    private string errorMessageField;

    private SFOEOrderTotLoadoutputOutheader[] arrayOutheaderField;

    private SFOEOrderTotLoadoutputOutline[] arrayOutlineField;

    private SFOEOrderTotLoadoutputOuttotal[] arrayOuttotalField;

    /// <remarks/>
    public string errorMessage
    {
        get { return this.errorMessageField; }
        set { this.errorMessageField = value; }
    }

    /// <remarks/>
    public SFOEOrderTotLoadoutputOutheader[] arrayOutheader
    {
        get { return this.arrayOutheaderField; }
        set { this.arrayOutheaderField = value; }
    }

    /// <remarks/>
    public SFOEOrderTotLoadoutputOutline[] arrayOutline
    {
        get { return this.arrayOutlineField; }
        set { this.arrayOutlineField = value; }
    }

    /// <remarks/>
    public SFOEOrderTotLoadoutputOuttotal[] arrayOuttotal
    {
        get { return this.arrayOuttotalField; }
        set { this.arrayOuttotalField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.input.Creditcard",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadinputCreditcard
{
    private string customerIDField;

    private string creditCardNumberField;

    private string paymentTypeField;

    private string creditCardExpirationField;

    private string cardHolderField;

    private string cvv2Field;

    private string address1Field;

    private string address2Field;

    private string address3Field;

    private string address4Field;

    private string cityField;

    private string stateField;

    private string zipCodeField;

    private string countryField;

    private string poNumberField;

    private string shipToZipCodeField;

    private double taxAmountField;

    private double authorizationAmountField;

    /// <remarks/>
    public string customerID
    {
        get { return this.customerIDField; }
        set { this.customerIDField = value; }
    }

    /// <remarks/>
    public string creditCardNumber
    {
        get { return this.creditCardNumberField; }
        set { this.creditCardNumberField = value; }
    }

    /// <remarks/>
    public string paymentType
    {
        get { return this.paymentTypeField; }
        set { this.paymentTypeField = value; }
    }

    /// <remarks/>
    public string creditCardExpiration
    {
        get { return this.creditCardExpirationField; }
        set { this.creditCardExpirationField = value; }
    }

    /// <remarks/>
    public string cardHolder
    {
        get { return this.cardHolderField; }
        set { this.cardHolderField = value; }
    }

    /// <remarks/>
    public string cvv2
    {
        get { return this.cvv2Field; }
        set { this.cvv2Field = value; }
    }

    /// <remarks/>
    public string address1
    {
        get { return this.address1Field; }
        set { this.address1Field = value; }
    }

    /// <remarks/>
    public string address2
    {
        get { return this.address2Field; }
        set { this.address2Field = value; }
    }

    /// <remarks/>
    public string address3
    {
        get { return this.address3Field; }
        set { this.address3Field = value; }
    }

    /// <remarks/>
    public string address4
    {
        get { return this.address4Field; }
        set { this.address4Field = value; }
    }

    /// <remarks/>
    public string city
    {
        get { return this.cityField; }
        set { this.cityField = value; }
    }

    /// <remarks/>
    public string state
    {
        get { return this.stateField; }
        set { this.stateField = value; }
    }

    /// <remarks/>
    public string zipCode
    {
        get { return this.zipCodeField; }
        set { this.zipCodeField = value; }
    }

    /// <remarks/>
    public string country
    {
        get { return this.countryField; }
        set { this.countryField = value; }
    }

    /// <remarks/>
    public string poNumber
    {
        get { return this.poNumberField; }
        set { this.poNumberField = value; }
    }

    /// <remarks/>
    public string shipToZipCode
    {
        get { return this.shipToZipCodeField; }
        set { this.shipToZipCodeField = value; }
    }

    /// <remarks/>
    public double taxAmount
    {
        get { return this.taxAmountField; }
        set { this.taxAmountField = value; }
    }

    /// <remarks/>
    public double authorizationAmount
    {
        get { return this.authorizationAmountField; }
        set { this.authorizationAmountField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.input.Inheader",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadinputInheader
{
    private double taxAmountField;

    private double authorizationAmountField;

    private string customerIDField;

    private string creditCardNumberField;

    private string paymentTypeField;

    private string creditCardExpirationField;

    private string warehouseIDField;

    private string orderSourceField;

    private string reviewOrderHoldField;

    private string poNumberField;

    private string ordNumberField;

    private string workStationField;

    private string billToContactField;

    private string billToCityField;

    private string billToStateField;

    private string billToZipCodeField;

    private string billToPhoneField;

    private string billToPhoneExtField;

    private string carrierCodeField;

    private string customerAddress1Field;

    private string customerAddress2Field;

    private string customerAddress3Field;

    private string customerAddress4Field;

    private string contractNumberField;

    private string customerNameField;

    private string customerCountryField;

    private string taxExemptCenturyField;

    private string taxExemptDateField;

    private string taxExCertNumberField;

    private string fobCodeField;

    private string requestedShipDateField;

    private string shipToAddress1Field;

    private string shipToAddress2Field;

    private string shipToAddress3Field;

    private string shipToAddress4Field;

    private string shipToContactField;

    private string shipToCityField;

    private string shipToCountryField;

    private string shipToNameField;

    private string shipToNumberField;

    private string shipToStateField;

    private string shipToPhoneField;

    private string shipToPhoneExtField;

    private string shipToZipCodeField;

    private string webTransactionTypeField;

    private string webProcessIDField;

    private string webTransactionIDField;

    private string webOrderIDField;

    private string freightMethodField;

    private string orderTypeField;

    private string quoteReviewDateField;

    /// <remarks/>
    public double taxAmount
    {
        get { return this.taxAmountField; }
        set { this.taxAmountField = value; }
    }

    /// <remarks/>
    public double authorizationAmount
    {
        get { return this.authorizationAmountField; }
        set { this.authorizationAmountField = value; }
    }

    /// <remarks/>
    public string customerID
    {
        get { return this.customerIDField; }
        set { this.customerIDField = value; }
    }

    /// <remarks/>
    public string creditCardNumber
    {
        get { return this.creditCardNumberField; }
        set { this.creditCardNumberField = value; }
    }

    /// <remarks/>
    public string paymentType
    {
        get { return this.paymentTypeField; }
        set { this.paymentTypeField = value; }
    }

    /// <remarks/>
    public string creditCardExpiration
    {
        get { return this.creditCardExpirationField; }
        set { this.creditCardExpirationField = value; }
    }

    /// <remarks/>
    public string warehouseID
    {
        get { return this.warehouseIDField; }
        set { this.warehouseIDField = value; }
    }

    /// <remarks/>
    public string orderSource
    {
        get { return this.orderSourceField; }
        set { this.orderSourceField = value; }
    }

    /// <remarks/>
    public string reviewOrderHold
    {
        get { return this.reviewOrderHoldField; }
        set { this.reviewOrderHoldField = value; }
    }

    /// <remarks/>
    public string poNumber
    {
        get { return this.poNumberField; }
        set { this.poNumberField = value; }
    }

    /// <remarks/>
    public string ordNumber
    {
        get { return this.ordNumberField; }
        set { this.ordNumberField = value; }
    }

    /// <remarks/>
    public string workStation
    {
        get { return this.workStationField; }
        set { this.workStationField = value; }
    }

    /// <remarks/>
    public string billToContact
    {
        get { return this.billToContactField; }
        set { this.billToContactField = value; }
    }

    /// <remarks/>
    public string billToCity
    {
        get { return this.billToCityField; }
        set { this.billToCityField = value; }
    }

    /// <remarks/>
    public string billToState
    {
        get { return this.billToStateField; }
        set { this.billToStateField = value; }
    }

    /// <remarks/>
    public string billToZipCode
    {
        get { return this.billToZipCodeField; }
        set { this.billToZipCodeField = value; }
    }

    /// <remarks/>
    public string billToPhone
    {
        get { return this.billToPhoneField; }
        set { this.billToPhoneField = value; }
    }

    /// <remarks/>
    public string billToPhoneExt
    {
        get { return this.billToPhoneExtField; }
        set { this.billToPhoneExtField = value; }
    }

    /// <remarks/>
    public string carrierCode
    {
        get { return this.carrierCodeField; }
        set { this.carrierCodeField = value; }
    }

    /// <remarks/>
    public string customerAddress1
    {
        get { return this.customerAddress1Field; }
        set { this.customerAddress1Field = value; }
    }

    /// <remarks/>
    public string customerAddress2
    {
        get { return this.customerAddress2Field; }
        set { this.customerAddress2Field = value; }
    }

    /// <remarks/>
    public string customerAddress3
    {
        get { return this.customerAddress3Field; }
        set { this.customerAddress3Field = value; }
    }

    /// <remarks/>
    public string customerAddress4
    {
        get { return this.customerAddress4Field; }
        set { this.customerAddress4Field = value; }
    }

    /// <remarks/>
    public string contractNumber
    {
        get { return this.contractNumberField; }
        set { this.contractNumberField = value; }
    }

    /// <remarks/>
    public string customerName
    {
        get { return this.customerNameField; }
        set { this.customerNameField = value; }
    }

    /// <remarks/>
    public string customerCountry
    {
        get { return this.customerCountryField; }
        set { this.customerCountryField = value; }
    }

    /// <remarks/>
    public string taxExemptCentury
    {
        get { return this.taxExemptCenturyField; }
        set { this.taxExemptCenturyField = value; }
    }

    /// <remarks/>
    public string taxExemptDate
    {
        get { return this.taxExemptDateField; }
        set { this.taxExemptDateField = value; }
    }

    /// <remarks/>
    public string taxExCertNumber
    {
        get { return this.taxExCertNumberField; }
        set { this.taxExCertNumberField = value; }
    }

    /// <remarks/>
    public string fobCode
    {
        get { return this.fobCodeField; }
        set { this.fobCodeField = value; }
    }

    /// <remarks/>
    public string requestedShipDate
    {
        get { return this.requestedShipDateField; }
        set { this.requestedShipDateField = value; }
    }

    /// <remarks/>
    public string shipToAddress1
    {
        get { return this.shipToAddress1Field; }
        set { this.shipToAddress1Field = value; }
    }

    /// <remarks/>
    public string shipToAddress2
    {
        get { return this.shipToAddress2Field; }
        set { this.shipToAddress2Field = value; }
    }

    /// <remarks/>
    public string shipToAddress3
    {
        get { return this.shipToAddress3Field; }
        set { this.shipToAddress3Field = value; }
    }

    /// <remarks/>
    public string shipToAddress4
    {
        get { return this.shipToAddress4Field; }
        set { this.shipToAddress4Field = value; }
    }

    /// <remarks/>
    public string shipToContact
    {
        get { return this.shipToContactField; }
        set { this.shipToContactField = value; }
    }

    /// <remarks/>
    public string shipToCity
    {
        get { return this.shipToCityField; }
        set { this.shipToCityField = value; }
    }

    /// <remarks/>
    public string shipToCountry
    {
        get { return this.shipToCountryField; }
        set { this.shipToCountryField = value; }
    }

    /// <remarks/>
    public string shipToName
    {
        get { return this.shipToNameField; }
        set { this.shipToNameField = value; }
    }

    /// <remarks/>
    public string shipToNumber
    {
        get { return this.shipToNumberField; }
        set { this.shipToNumberField = value; }
    }

    /// <remarks/>
    public string shipToState
    {
        get { return this.shipToStateField; }
        set { this.shipToStateField = value; }
    }

    /// <remarks/>
    public string shipToPhone
    {
        get { return this.shipToPhoneField; }
        set { this.shipToPhoneField = value; }
    }

    /// <remarks/>
    public string shipToPhoneExt
    {
        get { return this.shipToPhoneExtField; }
        set { this.shipToPhoneExtField = value; }
    }

    /// <remarks/>
    public string shipToZipCode
    {
        get { return this.shipToZipCodeField; }
        set { this.shipToZipCodeField = value; }
    }

    /// <remarks/>
    public string webTransactionType
    {
        get { return this.webTransactionTypeField; }
        set { this.webTransactionTypeField = value; }
    }

    /// <remarks/>
    public string webProcessID
    {
        get { return this.webProcessIDField; }
        set { this.webProcessIDField = value; }
    }

    /// <remarks/>
    public string webTransactionID
    {
        get { return this.webTransactionIDField; }
        set { this.webTransactionIDField = value; }
    }

    /// <remarks/>
    public string webOrderID
    {
        get { return this.webOrderIDField; }
        set { this.webOrderIDField = value; }
    }

    /// <remarks/>
    public string freightMethod
    {
        get { return this.freightMethodField; }
        set { this.freightMethodField = value; }
    }

    /// <remarks/>
    public string orderType
    {
        get { return this.orderTypeField; }
        set { this.orderTypeField = value; }
    }

    /// <remarks/>
    public string quoteReviewDate
    {
        get { return this.quoteReviewDateField; }
        set { this.quoteReviewDateField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.input.Inline",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadinputInline
{
    private string itemNumberField;

    private double orderQtyField;

    private string unitOfMeasureField;

    private string warehouseIDField;

    private string lineItemTypeField;

    private string itemDescription1Field;

    private string itemDesciption2Field;

    private double actualSellPriceField;

    private double costField;

    private string nonStockFlagField;

    private string chargeTypeField;

    private string dropShipField;

    private string dueDateField;

    private double extendedWeightField;

    private string listPriceField;

    private int itemIDField;

    private int sequenceNumberField;

    private string shipInstructionTypeField;

    /// <remarks/>
    public string itemNumber
    {
        get { return this.itemNumberField; }
        set { this.itemNumberField = value; }
    }

    /// <remarks/>
    public double orderQty
    {
        get { return this.orderQtyField; }
        set { this.orderQtyField = value; }
    }

    /// <remarks/>
    public string unitOfMeasure
    {
        get { return this.unitOfMeasureField; }
        set { this.unitOfMeasureField = value; }
    }

    /// <remarks/>
    public string warehouseID
    {
        get { return this.warehouseIDField; }
        set { this.warehouseIDField = value; }
    }

    /// <remarks/>
    public string lineItemType
    {
        get { return this.lineItemTypeField; }
        set { this.lineItemTypeField = value; }
    }

    /// <remarks/>
    public string itemDescription1
    {
        get { return this.itemDescription1Field; }
        set { this.itemDescription1Field = value; }
    }

    /// <remarks/>
    public string itemDesciption2
    {
        get { return this.itemDesciption2Field; }
        set { this.itemDesciption2Field = value; }
    }

    /// <remarks/>
    public double actualSellPrice
    {
        get { return this.actualSellPriceField; }
        set { this.actualSellPriceField = value; }
    }

    /// <remarks/>
    public double cost
    {
        get { return this.costField; }
        set { this.costField = value; }
    }

    /// <remarks/>
    public string nonStockFlag
    {
        get { return this.nonStockFlagField; }
        set { this.nonStockFlagField = value; }
    }

    /// <remarks/>
    public string chargeType
    {
        get { return this.chargeTypeField; }
        set { this.chargeTypeField = value; }
    }

    /// <remarks/>
    public string dropShip
    {
        get { return this.dropShipField; }
        set { this.dropShipField = value; }
    }

    /// <remarks/>
    public string dueDate
    {
        get { return this.dueDateField; }
        set { this.dueDateField = value; }
    }

    /// <remarks/>
    public double extendedWeight
    {
        get { return this.extendedWeightField; }
        set { this.extendedWeightField = value; }
    }

    /// <remarks/>
    public string listPrice
    {
        get { return this.listPriceField; }
        set { this.listPriceField = value; }
    }

    /// <remarks/>
    public int itemID
    {
        get { return this.itemIDField; }
        set { this.itemIDField = value; }
    }

    /// <remarks/>
    public int sequenceNumber
    {
        get { return this.sequenceNumberField; }
        set { this.sequenceNumberField = value; }
    }

    /// <remarks/>
    public string shipInstructionType
    {
        get { return this.shipInstructionTypeField; }
        set { this.shipInstructionTypeField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.output.Outheader",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadoutputOutheader
{
    private string messageOutField;

    private int orderNumberField;

    private int orderSuffixField;

    private string completionCodeField;

    private string custPONumberField;

    private double totalInvoiceAmountField;

    /// <remarks/>
    public string messageOut
    {
        get { return this.messageOutField; }
        set { this.messageOutField = value; }
    }

    /// <remarks/>
    public int orderNumber
    {
        get { return this.orderNumberField; }
        set { this.orderNumberField = value; }
    }

    /// <remarks/>
    public int orderSuffix
    {
        get { return this.orderSuffixField; }
        set { this.orderSuffixField = value; }
    }

    /// <remarks/>
    public string completionCode
    {
        get { return this.completionCodeField; }
        set { this.completionCodeField = value; }
    }

    /// <remarks/>
    public string custPONumber
    {
        get { return this.custPONumberField; }
        set { this.custPONumberField = value; }
    }

    /// <remarks/>
    public double totalInvoiceAmount
    {
        get { return this.totalInvoiceAmountField; }
        set { this.totalInvoiceAmountField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.output.Outline",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadoutputOutline
{
    private int orderNumberField;

    private int orderSuffixField;

    private int lineNumberField;

    private string itemNumberField;

    private string descriptionField;

    private double orderQtyField;

    private double shipQtyField;

    private double backorderQtyField;

    private double actualSellPriceField;

    private double lineAmountField;

    private string unitOfMeasureField;

    /// <remarks/>
    public int orderNumber
    {
        get { return this.orderNumberField; }
        set { this.orderNumberField = value; }
    }

    /// <remarks/>
    public int orderSuffix
    {
        get { return this.orderSuffixField; }
        set { this.orderSuffixField = value; }
    }

    /// <remarks/>
    public int lineNumber
    {
        get { return this.lineNumberField; }
        set { this.lineNumberField = value; }
    }

    /// <remarks/>
    public string itemNumber
    {
        get { return this.itemNumberField; }
        set { this.itemNumberField = value; }
    }

    /// <remarks/>
    public string description
    {
        get { return this.descriptionField; }
        set { this.descriptionField = value; }
    }

    /// <remarks/>
    public double orderQty
    {
        get { return this.orderQtyField; }
        set { this.orderQtyField = value; }
    }

    /// <remarks/>
    public double shipQty
    {
        get { return this.shipQtyField; }
        set { this.shipQtyField = value; }
    }

    /// <remarks/>
    public double backorderQty
    {
        get { return this.backorderQtyField; }
        set { this.backorderQtyField = value; }
    }

    /// <remarks/>
    public double actualSellPrice
    {
        get { return this.actualSellPriceField; }
        set { this.actualSellPriceField = value; }
    }

    /// <remarks/>
    public double lineAmount
    {
        get { return this.lineAmountField; }
        set { this.lineAmountField = value; }
    }

    /// <remarks/>
    public string unitOfMeasure
    {
        get { return this.unitOfMeasureField; }
        set { this.unitOfMeasureField = value; }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(
    TypeName = "SFOEOrderTotLoad.output.Outtotal",
    Namespace = "Nxtrend.WS"
)]
public partial class SFOEOrderTotLoadoutputOuttotal
{
    private string messageOutField;

    private double salesAmountField;

    private double totalSpecialChargeField;

    private double tradeDiscountAmountField;

    private double salesTaxAmountField;

    private double federalExciseAmountField;

    private double totalContainerAmountField;

    private double totalOrderValueField;

    private double totalInvoiceAmountField;

    private string currencyCodeField;

    private double amountAuthorizedField;

    private string completionCodeField;

    private double totalOrderWeightField;

    /// <remarks/>
    public string messageOut
    {
        get { return this.messageOutField; }
        set { this.messageOutField = value; }
    }

    /// <remarks/>
    public double salesAmount
    {
        get { return this.salesAmountField; }
        set { this.salesAmountField = value; }
    }

    /// <remarks/>
    public double totalSpecialCharge
    {
        get { return this.totalSpecialChargeField; }
        set { this.totalSpecialChargeField = value; }
    }

    /// <remarks/>
    public double tradeDiscountAmount
    {
        get { return this.tradeDiscountAmountField; }
        set { this.tradeDiscountAmountField = value; }
    }

    /// <remarks/>
    public double salesTaxAmount
    {
        get { return this.salesTaxAmountField; }
        set { this.salesTaxAmountField = value; }
    }

    /// <remarks/>
    public double federalExciseAmount
    {
        get { return this.federalExciseAmountField; }
        set { this.federalExciseAmountField = value; }
    }

    /// <remarks/>
    public double totalContainerAmount
    {
        get { return this.totalContainerAmountField; }
        set { this.totalContainerAmountField = value; }
    }

    /// <remarks/>
    public double totalOrderValue
    {
        get { return this.totalOrderValueField; }
        set { this.totalOrderValueField = value; }
    }

    /// <remarks/>
    public double totalInvoiceAmount
    {
        get { return this.totalInvoiceAmountField; }
        set { this.totalInvoiceAmountField = value; }
    }

    /// <remarks/>
    public string currencyCode
    {
        get { return this.currencyCodeField; }
        set { this.currencyCodeField = value; }
    }

    /// <remarks/>
    public double amountAuthorized
    {
        get { return this.amountAuthorizedField; }
        set { this.amountAuthorizedField = value; }
    }

    /// <remarks/>
    public string completionCode
    {
        get { return this.completionCodeField; }
        set { this.completionCodeField = value; }
    }

    /// <remarks/>
    public double totalOrderWeight
    {
        get { return this.totalOrderWeightField; }
        set { this.totalOrderWeightField = value; }
    }
}
