export declare global {
  export declare type Dictionary<TK, TV> = { [key in TK]: TV }

  export declare type LocalizableStringInfo = {
    name: string
    resourceName: string
  }

  export declare type ErrorMessage = {
    code: string
    details: string
    message: string
    validationErrors: ValidationError[]
  }

  export declare type ValidationError = {
    members: string[]
    message: string
  }

  export declare type ExtraPropertyDictionary = { [key: string]: any }

  export declare type IHasConcurrencyStamp = {
    concurrencyStamp?: string
  }

  export declare type IHasExtraProperties = {
    extraProperties: ExtraPropertyDictionary
  }

  export declare type ISelectionStringValueItem = {
    displayText: LocalizableStringInfo
    value: string
  }

  export declare type ISelectionStringValueItemSource = {
    items: ISelectionStringValueItem[]
  }

  export declare type Validator = {
    name: string
    properties: { [key: string]: string }
  }

  export declare type ValueType = {
    name: string
    properties: { [key: string]: string }
    validator: Validator
  }

  export declare type SelectionStringValueType = ValueType & {
    itemSource: ISelectionStringValueItemSource
  }

  export declare type LanguageInfo = {
    cultureName: string
    displayName: string
    flagIcon?: string
    twoLetterISOLanguageName: string
    uiCultureName: string
  }

  export declare type DateTimeFormat = {
    calendarAlgorithmType: string
    dateSeparator: string
    dateTimeFormatLong: string
    fullDateTimePattern: string
    longTimePattern: string
    shortDatePattern: string
    shortTimePattern: string
  }

  export declare type CurrentCulture = {
    cultureName: string
    dateTimeFormat: DateTimeFormat
    displayName: string
    englishName: string
    isRightToLeft: boolean
    name: string
    nativeName: string
    threeLetterIsoLanguageName: string
    twoLetterIsoLanguageName: string
  }

  export declare type WindowsTimeZone = {
    timeZoneId: string
  }

  export declare type IanaTimeZone = {
    timeZoneName: string
  }

  export declare type TimeZone = {
    iana: IanaTimeZone
    windows: WindowsTimeZone
  }

  export declare type Clock = {
    kind: string
  }

  export declare type MultiTenancyInfo = {
    isEnabled: boolean
  }

  export declare type CurrentTenant = {
    id?: string
    isAvailable: boolean
    name?: string
  }

  export declare type CurrentUser = {
    email: string
    emailVerified: boolean
    id?: string
    impersonatorTenantId?: string
    impersonatorTenantName?: string
    impersonatorUserId?: string
    impersonatorUserName?: string
    isAuthenticated: boolean
    name?: string
    phoneNumber?: string
    phoneNumberVerified: boolean
    roles: string[]
    surName?: string
    tenantId?: string
    userName: string
  }

  export declare type ExtensibleObject = {
    extraProperties: ExtraPropertyDictionary
  }

  export declare type EntityDto<TPrimaryKey> = {
    id: TPrimaryKey
  }

  export declare type CreationAuditedEntityDto<TPrimaryKey> =
    EntityDto<TPrimaryKey> & {
      creationTime: string
      creatorId?: string
    }

  export declare type CreationAuditedEntityWithUserDto<TPrimaryKey, TUserDto> =
    CreationAuditedEntityDto<TPrimaryKey> & {
      creator: TUserDto
    }

  export declare type AuditedEntityDto<TPrimaryKey> =
    CreationAuditedEntityDto<TPrimaryKey> & {
      lastModificationTime?: string
      lastModifierId?: string
    }

  export declare type AuditedEntityWithUserDto<TPrimaryKey, TUserDto> =
    AuditedEntityDto<TPrimaryKey> & {
      creator: TUserDto
      lastModifier: TUserDto
    }

  export declare type ExtensibleEntityDto<TKey> = ExtensibleObject & {
    id: TKey
  }

  export declare type ExtensibleCreationAuditedEntityDto<TPrimaryKey> =
    ExtensibleEntityDto<TPrimaryKey> & {
      creationTime: string
      creatorId?: string
    }

  export declare type ExtensibleCreationAuditedEntityWithUserDto<
    TPrimaryKey,
    TUserDto,
  > = ExtensibleCreationAuditedEntityDto<TPrimaryKey> & {
    creator: TUserDto
  }

  export declare type ExtensibleAuditedEntityDto<TPrimaryKey> =
    ExtensibleCreationAuditedEntityDto<TPrimaryKey> & {
      lastModificationTime?: string
      lastModifierId?: string
    }

  export declare type ExtensibleAuditedEntityWithUserDto<
    TPrimaryKey,
    TUserDto,
  > = ExtensibleAuditedEntityDto<TPrimaryKey> & {
    creator: TUserDto
    lastModifier: TUserDto
  }

  export declare type ExtensibleFullAuditedEntityDto<TPrimaryKey> =
    ExtensibleAuditedEntityDto<TPrimaryKey> & {
      deleterId?: string
      deletionTime?: string
      isDeleted: boolean
    }

  export declare type ExtensibleFullAuditedEntityWithUserDto<
    TPrimaryKey,
    TUserDto,
  > = ExtensibleFullAuditedEntityDto<TPrimaryKey> & {
    creator: TUserDto
    deleter: TUserDto
    lastModifier: TUserDto
  }

  export declare type FullAuditedEntityDto<TPrimaryKey = string> =
    AuditedEntityDto<TPrimaryKey> & {
      deleterId?: string
      deletionTime?: string
      isDeleted: boolean
    }

  export declare type FullAuditedEntityWithUserDto<TPrimaryKey, TUserDto> =
    FullAuditedEntityDto<TPrimaryKey> & {
      creator: TUserDto
      deleter: TUserDto
      lastModifier: TUserDto
    }

  export declare type LimitedResultRequestDto = {
    maxResultCount?: number
  }

  export declare type ExtensibleLimitedResultRequestDto = ExtensibleObject &
    LimitedResultRequestDto

  export declare type ListResultDto<T> = {
    items: T[]
  }

  export declare type ExtensibleListResultDto<T> = ExtensibleObject &
    ListResultDto<T>

  export declare type PagedResultDto<T> = ListResultDto<T> & {
    totalCount: number
  }

  export declare type ExtensiblePagedResultDto<T> = ExtensibleObject &
    PagedResultDto<T>

  export declare type SortedResultRequest = {
    sorting?: string
  }

  export declare type PagedAndSortedResultRequestDto = PagedResultRequestDto &
    SortedResultRequest

  export declare type ExtensiblePagedAndSortedResultRequestDto =
    ExtensibleObject & PagedAndSortedResultRequestDto

  export declare type PagedResultRequestDto = LimitedResultRequestDto & {
    skipCount?: number
  }

  export declare type ExtensiblePagedResultRequestDto = ExtensibleObject &
    PagedResultRequestDto

  export declare type ApplicationLocalizationResourceDto = {
    baseResources: string[]
    texts: Dictionary<string, string>
  }

  export declare type ApplicationLocalizationDto = {
    resources: Dictionary<string, ApplicationLocalizationResourceDto>
  }

  export declare type ApplicationLocalizationConfigurationDto = {
    currentCulture: CurrentCulture
    defaultResourceName: string
    languageFilesMap: Dictionary<string, NameValue[]>
    languages: LanguageInfo[]
    languagesMap: Dictionary<string, NameValue[]>
    resources: Dictionary<string, ApplicationLocalizationResourceDto>
    values: Dictionary<string, Dictionary<string, string>>
  }

  export declare type ApplicationAuthConfigurationDto = {
    grantedPolicies: Dictionary<string, boolean>
  }

  export declare type ApplicationSettingConfigurationDto = {
    values: Dictionary<string, string>
  }

  export declare type ApplicationFeatureConfigurationDto = {
    values: Dictionary<string, string>
  }

  export declare type ApplicationGlobalFeatureConfigurationDto = {
    enabledFeatures: string[]
  }

  export declare type TimingDto = {
    timeZone: TimeZone
  }

  export declare type LocalizableStringDto = {
    name: string
    resource?: string
  }

  export declare type ExtensionPropertyApiGetDto = {
    isAvailable: boolean
  }

  export declare type ExtensionPropertyApiCreateDto = {
    isAvailable: boolean
  }

  export declare type ExtensionPropertyApiUpdateDto = {
    isAvailable: boolean
  }

  export declare type ExtensionPropertyUiTableDto = {
    isAvailable: boolean
  }

  export declare type ExtensionPropertyUiFormDto = {
    isAvailable: boolean
  }

  export declare type ExtensionPropertyUiLookupDto = {
    displayPropertyName: string
    filterParamName: string
    resultListPropertyName: string
    url: string
    valuePropertyName: string
  }

  export declare type ExtensionPropertyApiDto = {
    onCreate: ExtensionPropertyApiCreateDto
    onGet: ExtensionPropertyApiGetDto
    onUpdate: ExtensionPropertyApiUpdateDto
  }

  export declare type ExtensionPropertyUiDto = {
    lookup: ExtensionPropertyUiLookupDto
    onCreateForm: ExtensionPropertyUiFormDto
    onEditForm: ExtensionPropertyUiFormDto
    onTable: ExtensionPropertyUiTableDto
  }

  export declare type ExtensionPropertyAttributeDto = {
    config: Dictionary<string, any>
    typeSimple: string
  }

  export declare type ExtensionPropertyDto = {
    api: ExtensionPropertyApiDto
    attributes: ExtensionPropertyAttributeDto[]
    configuration: Dictionary<string, any>
    defaultValue: any
    displayName?: LocalizableStringDto
    type: string
    typeSimple: string
    ui: ExtensionPropertyUiDto
  }

  export declare type EntityExtensionDto = {
    configuration: Dictionary<string, any>
    properties: Dictionary<string, ExtensionPropertyDto>
  }

  export declare type ModuleExtensionDto = {
    configuration: Dictionary<string, any>
    entities: Dictionary<string, EntityExtensionDto>
  }

  export declare type ExtensionEnumFieldDto = {
    name: string
    value: any
  }

  export declare type ExtensionEnumDto = {
    fields: ExtensionEnumFieldDto[]
    localizationResource: string
  }

  export declare type ObjectExtensionsDto = {
    enums: Dictionary<string, ExtensionEnumDto>
    modules: Dictionary<string, ModuleExtensionDto>
  }

  export declare type ApplicationConfigurationDto = IHasExtraProperties & {
    auth: ApplicationAuthConfigurationDto
    clock: Clock
    currentTenant: CurrentTenant
    currentUser: CurrentUser
    features: ApplicationFeatureConfigurationDto
    globalFeatures: ApplicationGlobalFeatureConfigurationDto
    localization: ApplicationLocalizationConfigurationDto
    multiTenancy: MultiTenancyInfo
    objectExtensions: ObjectExtensionsDto
    setting: ApplicationSettingConfigurationDto
    timing: Timing
  }
}
