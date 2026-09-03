# HNTAS.Api.Client.Api.HeatNetworksApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiHeatNetworksAddHeatNetworkPost**](HeatNetworksApi.md#apiheatnetworksaddheatnetworkpost) | **POST** /api/HeatNetworks/add-heat-network |  |
| [**ApiHeatNetworksExistingNetworkByUserIdGet**](HeatNetworksApi.md#apiheatnetworksexistingnetworkbyuseridget) | **GET** /api/HeatNetworks/existing-network-by-userId |  |
| [**ApiHeatNetworksGet**](HeatNetworksApi.md#apiheatnetworksget) | **GET** /api/HeatNetworks |  |
| [**ApiHeatNetworksHeatNetworkByUserIdGet**](HeatNetworksApi.md#apiheatnetworksheatnetworkbyuseridget) | **GET** /api/HeatNetworks/heat-network-by-userId |  |
| [**ApiHeatNetworksHeatNetworkByUserIdPaginatedGet**](HeatNetworksApi.md#apiheatnetworksheatnetworkbyuseridpaginatedget) | **GET** /api/HeatNetworks/heat-network-by-userId-paginated |  |
| [**ApiHeatNetworksHnIdGet**](HeatNetworksApi.md#apiheatnetworkshnidget) | **GET** /api/HeatNetworks/{hnId} |  |
| [**ApiHeatNetworksHnIdsGet**](HeatNetworksApi.md#apiheatnetworkshnidsget) | **GET** /api/HeatNetworks/hnIds |  |
| [**ApiHeatNetworksNetworkElementsPut**](HeatNetworksApi.md#apiheatnetworksnetworkelementsput) | **PUT** /api/HeatNetworks/network-elements |  |
| [**ApiHeatNetworksRegisterOfgemNetworkPut**](HeatNetworksApi.md#apiheatnetworksregisterofgemnetworkput) | **PUT** /api/HeatNetworks/register-ofgem-network |  |
| [**ExternalHeatNetworkHnIdGet**](HeatNetworksApi.md#externalheatnetworkhnidget) | **GET** /external/heat-network/{hnId} |  |
| [**ExternalHeatNetworksGet**](HeatNetworksApi.md#externalheatnetworksget) | **GET** /external/heat-networks |  |
| [**ExternalHeatNetworksSearchGet**](HeatNetworksApi.md#externalheatnetworkssearchget) | **GET** /external/heat-networks/search |  |

<a id="apiheatnetworksaddheatnetworkpost"></a>
# **ApiHeatNetworksAddHeatNetworkPost**
> HeatNetworkResponse ApiHeatNetworksAddHeatNetworkPost (HeatNetwork heatNetwork)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **heatNetwork** | [**HeatNetwork**](HeatNetwork.md) |  |  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksexistingnetworkbyuseridget"></a>
# **ApiHeatNetworksExistingNetworkByUserIdGet**
> ExistingNetworkResponse ApiHeatNetworksExistingNetworkByUserIdGet (ExistingNetworkRequest existingNetworkRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **existingNetworkRequest** | [**ExistingNetworkRequest**](ExistingNetworkRequest.md) |  |  |

### Return type

[**ExistingNetworkResponse**](ExistingNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksget"></a>
# **ApiHeatNetworksGet**
> List&lt;HeatNetworkResponse&gt; ApiHeatNetworksGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;HeatNetworkResponse&gt;**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksheatnetworkbyuseridget"></a>
# **ApiHeatNetworksHeatNetworkByUserIdGet**
> List&lt;HeatNetworkResponse&gt; ApiHeatNetworksHeatNetworkByUserIdGet (string userId = null, RegistrationSource2 registrationSource = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |
| **registrationSource** | **RegistrationSource2** |  | [optional]  |

### Return type

[**List&lt;HeatNetworkResponse&gt;**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksheatnetworkbyuseridpaginatedget"></a>
# **ApiHeatNetworksHeatNetworkByUserIdPaginatedGet**
> PagedResultOfHeatNetworkResponse ApiHeatNetworksHeatNetworkByUserIdPaginatedGet (string userId = null, RegistrationSource2 registrationSource = null, int pageNumber = null, int pageSize = null, string sortBy = null, string sortDirection = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **userId** | **string** |  | [optional]  |
| **registrationSource** | **RegistrationSource2** |  | [optional]  |
| **pageNumber** | **int** |  | [optional] [default to 1] |
| **pageSize** | **int** |  | [optional] [default to 10] |
| **sortBy** | **string** |  | [optional] [default to &quot;Name&quot;] |
| **sortDirection** | **string** |  | [optional] [default to &quot;asc&quot;] |

### Return type

[**PagedResultOfHeatNetworkResponse**](PagedResultOfHeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworkshnidget"></a>
# **ApiHeatNetworksHnIdGet**
> HeatNetworkResponse ApiHeatNetworksHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworkshnidsget"></a>
# **ApiHeatNetworksHnIdsGet**
> List&lt;HeatNetworkResponse&gt; ApiHeatNetworksHnIdsGet (string hnIdsString = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnIdsString** | **string** |  | [optional]  |

### Return type

[**List&lt;HeatNetworkResponse&gt;**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksnetworkelementsput"></a>
# **ApiHeatNetworksNetworkElementsPut**
> HeatNetworkResponse ApiHeatNetworksNetworkElementsPut (NetworkElements2 networkElements2, string hnId = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **networkElements2** | [**NetworkElements2**](NetworkElements2.md) |  |  |
| **hnId** | **string** |  | [optional]  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksregisterofgemnetworkput"></a>
# **ApiHeatNetworksRegisterOfgemNetworkPut**
> HeatNetworkResponse ApiHeatNetworksRegisterOfgemNetworkPut (HeatNetwork heatNetwork)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **heatNetwork** | [**HeatNetwork**](HeatNetwork.md) |  |  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="externalheatnetworkhnidget"></a>
# **ExternalHeatNetworkHnIdGet**
> HeatNetworkExternalResponse ExternalHeatNetworkHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**HeatNetworkExternalResponse**](HeatNetworkExternalResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="externalheatnetworksget"></a>
# **ExternalHeatNetworksGet**
> List&lt;HeatNetworkExternalResponse&gt; ExternalHeatNetworksGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;HeatNetworkExternalResponse&gt;**](HeatNetworkExternalResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="externalheatnetworkssearchget"></a>
# **ExternalHeatNetworksSearchGet**
> List&lt;HeatNetworkExternalResponse&gt; ExternalHeatNetworksSearchGet (DateTimeOffset fromDate = null, DateTimeOffset toDate = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fromDate** | **DateTimeOffset** |  | [optional]  |
| **toDate** | **DateTimeOffset** |  | [optional]  |

### Return type

[**List&lt;HeatNetworkExternalResponse&gt;**](HeatNetworkExternalResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

