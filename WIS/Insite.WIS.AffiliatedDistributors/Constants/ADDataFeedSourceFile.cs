namespace Insite.WIS.AffiliatedDistributors.Constants;

public static class ADDataFeedSourceFile
{
    public const string AttributeNameColumn = "ATTRIBUTE_NAME";

    public const string AttributeUomColumn = "ATTRIBUTE_UOM";

    public const string AttributeValueColumn = "ATTRIBUTE_VALUE";

    public const string AttributeValueUomColumn = "Value_UOM";

    public const string CategoryCodeColumnFormat = "CATEGORY_CODE_{0}_LEVEL_{1}";

    public const string CategoryCodeColumnFormatNoCategoryNumber = "CATEGORY_CODE_LEVEL_{0}";

    public const string CategoryColumn = "CATEGORY";

    public const string CategoryNameColumnFormat = "CATEGORY_NAME_{0}_LEVEL_{1}";

    public const string CategoryNameColumnFormatNoCategoryNumber = "CATEGORY_NAME_LEVEL_{0}";

    public const string DifferentiatorColumn = "DIFFERENTIATOR";

    public const string DocCaptionColumn = "DOC_CAPTION";

    public const string ItemDocumentNameColumn = "ITEM_DOCUMENT_NAME";

    public const string ItemDocumentTypeColumn = "ITEM_DOCUMENT_TYPE";

    public const string ItemFeaturesColumn = "ITEM_FEATURES";

    public const string ItemImageCaptionColumn = "ITEM_IMAGE_CAPTION";

    public const string ItemImageDetailImageColumn = "ITEM_IMAGE_DETAILIMAGE";

    public const string ItemImageEnlargeImageColumn = "ITEM_IMAGE_ENLARGEIMAGE";

    public const string ItemImageItemImageColumn = "ITEM_IMAGE_ITEMIMAGE";

    public const string ItemImageFilenameColumn = "Image_Filename";

    public const string ItemSmallImageFilenameColumn = "Image_Small";

    public const string ItemMediumImageFilenameColumn = "Image_Medium";

    public const string ItemLargeImageFilenameColumn = "Image_Large";

    public const string SkuShortDescriptionColumn = "SKU_Short_Description";

    public const string CategoryImageName = "CategoryImageName";

    public const string LevelNumber = "LevelNumber";

    public const string LevelCodeColumnFormat = "Level{0}Code";

    /// <summary>
    /// Part number column for the EnterWorks PIM
    /// </summary>
    public const string MemberPartNumberColumn = "Member_Part_Number";
    public const string MemberPartNumberColumnSpaceDelimited = "Member Part Number";

    /// <summary>
    /// AD Data feed EnterWorks PIM
    /// </summary>
    public const string NumericCodeColumn = "NumericCode";

    public const string StyleClassColumn = "StyleClass";

    public const string StyleTraitColumn = "StyleTrait";

    public const string CategoryCodeColumn = "Category_Code";

    public const string ProductNameColumn = "Product_Name";

    public const string ProductGroupColumn = "Product_Group";

    public const string StyleTraitValueColumn = "StyleTraitValue";

    public const string NodeNameColumn = "NodeName";

    public const string DisplaySequenceColumn = "DisplaySequence";

    public const string TaxonomyNameColumn = "TaxonomyName";

    public const string CategoryAttributeNameColumn = "AttributeName";

    public const string DataTypeColumn = "DataType";

    public const string CategoryAttributeValueColumn = "AttributeValue";

    public const string ValueDelimeterColumn = "ValueDelimeter";

    public const string IsDifferentiatorColumn = "IsDifferentiator";

    public const string EntityTypeColumn = "EntityType";

    public const string IsFilterableColumn = "IsFilterable";

    public const string FilterSequenceColumn = "FilterSequence";

    public const string PrintColumn = "Print";

    public const string StatusColumn = "Status";

    public const string ApplicationColumn = "Application";

    public const string IncludesColumn = "Includes";

    public const string StandardsApprovalsColumn = "Standards_Approvals";

    public const string VideoLinksColumn = "Video_Links";

    public const string WarrantyColumn = "Warranty";

    public const string AlternatePartNumberColumn = "Alternative_Part_Number";

    public const string BrandedReferencePartNumberColumn = "Branded_Reference_Part_Number";

    public const string DiscontinuedManufacturerPartNumberColumn =
        "Discontinued_Manufacturer_Part_Number";

    public const string SKULongDescription1Column = "SKU_Long_Description_1";

    public const string SKULongDescription2Column = "SKU_Long_Description_2";

    public const string PartNumberKeywordsColumn = "Part_Number_Keywords";

    public const string CompetitorColumn = "Competitor";

    public const string WholesalerColumn = "Wholesaler";

    public const string BrandNameColumn = "Brand_Name";

    public const string EANColumn = "EAN";

    public const string GTINColumn = "GTIN";

    public const string UPCColumn = "UPC";

    public const string CountryOfOriginColumn = "Country_Of_Origin";

    public const string ManufacturerNameColumn = "Manufacturer_Name";

    public const string MarketingDescriptionColumn = "Marketing_Description";

    public const string CAPriceColumn = "CA_Price_List_Price_Amount";

    public const string CAPriceActivationColumn = "CA_Price_Effective_Start_Date";

    public const string CAPriceDeactivationColumn = "CA_Price_Effective_End_Date";

    public const string USPriceColumn = "US_Price_List_Price_Amount";

    public const string USPriceActivationColumn = "US_Price_Effective_Start_Date";

    public const string USPriceDeactivationColumn = "US_Price_Effective_End_Date";

    public const string SalesUnitOfMeasureColumn = "SalesUOM";

    /// <summary>
    /// Part number column for the Unilog CX1 PIM
    /// </summary>
    public const string MyPartNumberColumn = "MY_PART_NUMBER";

    public const string VariantParent = "VariantParent";

    public const string VariantType = "VariantType";

    public const string LanguageCode = "LanguageCode";

    public const string ThreeSixtyImageColumn = "360_IMAGE";

    public const string ThreeSixtyImageUrlColumn = "360_Spin_URL";

    public const string Images = "Images";

    public const string EnterworksDocCaptionColumn = "Document_Caption";

    public const string EnterworksItemDocumentNameColumn = "Document_Filename";

    public const string Keywords = "Keywords";

    /// <summary>
    /// List of AD fields used to aggregate MetaKeywords mapping
    /// </summary>
    public static readonly string[] MetaKeywordsFields =
    {
        Keywords,
        PartNumberKeywordsColumn,
        AlternatePartNumberColumn,
        BrandedReferencePartNumberColumn,
        DiscontinuedManufacturerPartNumberColumn,
        GTINColumn,
        CompetitorColumn,
        WholesalerColumn
    };

    public const string MetaDescription = "MetaDescription";

    /// <summary>
    /// List of AD fields used to aggregate MetaDescription mapping
    /// </summary>
    public static readonly string[] MetaDescriptionFields =
    {
        SKULongDescription1Column,
        SKULongDescription2Column
    };
}
