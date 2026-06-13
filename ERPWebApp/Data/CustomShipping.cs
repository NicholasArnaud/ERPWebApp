using ERPWebApp.Models.Orders;
using static ERPWebApp.Data.CustomShipping;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data;

public interface ICustomShippingBuilder
{
    long StoreId { get; }
    OrderShippingInfo OrderShippingInfo { get; }
    Dictionary<string, ShipperApi> AppliedShipperIds { get; }
    OrderInsuranceOptions InsuranceOptions { get; }
    OrderShippingInfo DefaultShippingInfo { get; }
    bool AllowedPreferredServiceCode { get; }
    ICustomShippingBuilder SetStoreId(long storeId);
    ICustomShippingBuilder SetDefaultShippingInfo();
    ICustomShippingBuilder UpdateValidShipperIdsByWeight(OrderWeight weight);
    ICustomShippingBuilder UpdateValidShipperIdsByDimensions(OrderDimensions dimensions);
    ICustomShippingBuilder UpdateValidShipperIdsByShippingInfo(OrderShippingInfo destination);
    ICustomShippingBuilder UpdateValidShipperIdsByItems(List<OrderItem> orderItems);
    ICustomShippingBuilder UpdateValidShipperIdsByOrder(Order order);
    ICustomShippingBuilder RemoveInvalidShippingServicesByShippingEstimates(List<ShipEngineShippingEstimate> shipEngineShippingEstimates);
    ICustomShippingBuilder AddEnsuredValidServiceCodesByShippingEstimates(List<ShipEngineShippingEstimate> shipEngineShippingEstimates);
    ICustomShippingBuilder SetTemporaryHoldOnService();
    CustomShipping Build();
}
public class CustomShippingBuilder : ICustomShippingBuilder
{
    public long StoreId => _storeId;
    public OrderShippingInfo OrderShippingInfo => _orderShippingInfo;
    public Dictionary<string, ShipperApi> AppliedShipperIds => _appliedShipperIds;
    public OrderInsuranceOptions InsuranceOptions => _insuranceOptions;
    public OrderShippingInfo DefaultShippingInfo => _defaultShippingInfo;
    public bool AllowedPreferredServiceCode => _allowedPreferredService;

    private long _storeId;
    private bool _expedited { get; set; } = false;
    private bool _allowedPreferredService { get; set; } = false;
    private string _setPreferredServiceCode { get; set; }
    private OrderShippingInfo _orderShippingInfo;
    private OrderShippingInfo _defaultShippingInfo;
    private Dictionary<string, ShipperApi> _appliedShipperIds;
    private OrderInsuranceOptions _insuranceOptions;
    private readonly ShippingCarriers _shipEngineShippingCarriers = new(shipperApi: ShipperApi.ShipEngine);
    private readonly ShippingCarriers _shipStationShippingCarriers = new(shipperApi: ShipperApi.ShipStation);
    // list of US territories and outlying island codes as listed for all carriers on ShipStation API site
    private static readonly List<string> _territoryAndIslandCodes =
    [
        "as", // American Samoa
        "american samoa",
        "gu", //Guam
        "guam",
        "mp", // Northern Mariana Islands
        "pw", // Palau
        "pr", // Puerto Rico
        "fm", // Micronesia (sometimes has US as country code for USPS)
        "vi", // U.S. Virgin Islands
        "virgin islands",
        "mh", // Marshall Islands
        "um", // US Minor Outlying Islands including: Baker Island, Howland Island, Jarvis Island, Johnston Atoll,
        // Kingman Reef, Midway Islands, Palmyra Atoll, Navassa Island, Wake Island
        "puerto rico" // (+Puerto Rico spelled out because that exists in past orders in this field a few times)
    ];
    private sealed record ShippingCarriers
    {
        public readonly string UPSGroundPlus;
        public readonly string UPSGroundSurePost;
        public readonly string DesignsDirectUPS;
        public readonly string MichaelsUPS;
        public readonly string StampsUSPS;
        public readonly string InternalFedEx;
        public readonly string WayFairFedEx;
        public readonly string KirklandFedEx;
        public readonly string OSM;
        public readonly string DHL;

        public ShippingCarriers(ShipperApi shipperApi)
        {
            switch (shipperApi)
            {
                case ShipperApi.ShipStation:
                    {
                        UPSGroundPlus = "1317620";
                        UPSGroundSurePost = "1317612";
                        StampsUSPS = "205318";
                        InternalFedEx = "1361220";
                        OSM = "1510102";
                        break;
                    }
                case ShipperApi.ShipEngine:
                    {
                        UPSGroundPlus = "se-3191868";
                        UPSGroundSurePost = "se-3191854";
                        StampsUSPS = "se-548261";
                        InternalFedEx = "se-635658";
                        OSM = "se-5880824";
                        DesignsDirectUPS = "se-5993332";
                        MichaelsUPS = "se-3528951";
                        WayFairFedEx = "se-635658";//Our Fedex account but billed 3-rd party to Wayfair
                        KirklandFedEx = "se-3443168";
                        DHL = "se-2159546";
                        break;
                    }
                default:
                    break;
            }

        }

    }

    public ICustomShippingBuilder SetStoreId(long storeId)
    {
        _storeId = storeId;
        switch (storeId)
        {
            //CustomERP Account
            case 809491 or 980691 or 798762 or 973395 or 993301 or 1021686 or 286701 or 1038074:
                {
                    _orderShippingInfo = new OrderShippingInfo()
                    {
                        name = "Fulfillment Center",
                        company = "Fulfillment Center",
                        city = "Lafayette",
                        country = "US",
                        state = "LA",
                        street1 = "1229 NW Evangeline Thruway",
                        phone = "318-623-5046",
                        postalCode = "70501",
                        residential = false,
                        street2 = "",
                        street3 = ""
                    };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>() {
                        {_shipEngineShippingCarriers.UPSGroundPlus, ShipperApi.ShipEngine},
                        {_shipEngineShippingCarriers.UPSGroundSurePost, ShipperApi.ShipEngine},
                        {_shipEngineShippingCarriers.StampsUSPS, ShipperApi.ShipEngine},
                        {_shipEngineShippingCarriers.InternalFedEx, ShipperApi.ShipEngine},
                        {_shipEngineShippingCarriers.DHL, ShipperApi.ShipEngine},
                        {_shipStationShippingCarriers.UPSGroundPlus, ShipperApi.ShipStation},
                        {_shipStationShippingCarriers.UPSGroundSurePost, ShipperApi.ShipStation},
                        {_shipStationShippingCarriers.StampsUSPS, ShipperApi.ShipStation},
                        {_shipStationShippingCarriers.InternalFedEx, ShipperApi.ShipStation}
                    };
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = true,
                        insuredValue = 0.99m,
                        provider = OrderInsuranceOptions.Provider.shipsurance
                    };

                    switch (_storeId)
                    {
                        case 809491://Falls into CustomERP
                            {
                                break;
                            }
                        //CustomizedShed - AMAZON STORE
                        case 993301:
                            {
                                //AMAZON FORBIDS OSM
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
                                //Doesn't Insure
                                _insuranceOptions.insureShipment = false;
                                _insuranceOptions.insuredValue = 0.0m;
                                _insuranceOptions.provider = OrderInsuranceOptions.Provider.none;
                                break;
                            }
                        //CreativeNCustomized AND Unique&Yours
                        case 973395 or 798762:
                            {
                                //Eliminate OSM
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
                                //Eliminate UPS 11+
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundPlus);
                                //Eliminate UPS GROUND
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundSurePost);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundSurePost);
                                break;
                            }
                        //Fanrock
                        case 1002300:
                            {
                                //Eliminate STAMPS/USPS
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.StampsUSPS);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.StampsUSPS);
                                //Eliminate UPS 11+
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundPlus);
                                //Eliminate UPS GROUND
                                _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundSurePost);
                                _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundSurePost);
                                //Doesn't Insure
                                _insuranceOptions.insureShipment = false;
                                _allowedPreferredService = true;
                                _insuranceOptions.insuredValue = 0.0m;
                                _insuranceOptions.provider = OrderInsuranceOptions.Provider.none;
                                break;
                            }
                        //Preferred shipping enabled stores
                        //Q2 aka Qualtry AND First1 (Yours Personalized) AND Boutiqueandpours
                        case 1021686 or 286701 or 1038074:
                            {
                                //Doesn't Insure
                                _insuranceOptions.insureShipment = false;
                                //Allowed requested service
                                _allowedPreferredService = true;
                                _insuranceOptions.insuredValue = 0.0m;
                                _insuranceOptions.provider = OrderInsuranceOptions.Provider.none;
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                }
            //Michaels DesignsDirect Account
            case 1005916:
                {
                    _orderShippingInfo = new OrderShippingInfo()
                    {
                        name = "Michaels Returns",
                        company = "Michaels",
                        street1 = "860 Westport Pkwy",
                        street2 = "",
                        street3 = "",
                        city = "Fort Worth",
                        state = "TX",
                        postalCode = "76177",
                        country = "US",
                        phone = "800-642-4235",
                        residential = false
                    };
                    //_appliedAccountNumbers = new List<string>() { "633641757" };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>()
                    {
                        {_shipEngineShippingCarriers.MichaelsUPS , ShipperApi.ShipEngine }
                    };
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = false,
                        insuredValue = 0.00m,
                        provider = OrderInsuranceOptions.Provider.none
                    };
                    break;
                }
            //Kirklands DesignsDirect Account
            case 1001347:
                {
                    _orderShippingInfo = new OrderShippingInfo()
                    {
                        name = "Kirkland's",
                        company = "Kirkland's",
                        street1 = "5310 Maryland Way",
                        street2 = "",
                        street3 = "",
                        city = "BrentWood",
                        state = "TN",
                        postalCode = "37027",
                        country = "US",
                        phone = "877-541-4855",
                        residential = false
                    };
                    //_appliedAccountNumbers = new List<string>() { "633641757" };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>()
                    {
                        //KIRKLANDS FEDEX
                        {_shipEngineShippingCarriers.KirklandFedEx , ShipperApi.ShipEngine }
                    };
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = false,
                        insuredValue = 0.00m,
                        provider = OrderInsuranceOptions.Provider.none
                    };
                    break;
                }
            //Wayfair DesignsDirect Account
            case 1002827:
                {
                    _orderShippingInfo = new OrderShippingInfo()
                    {
                        name = "Wayfair",
                        company = "Wayfair",
                        street1 = "5101 Renegade Way",
                        street2 = "",
                        street3 = "",
                        city = "Florence",
                        state = "KY",
                        postalCode = "41042",
                        country = "US",
                        phone = "877-929-3247",
                        residential = false
                    };
                    //_appliedAccountNumbers = new List<string>() { "346593300" };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>()
                    {
                        //WAYFAIR FEDEX
                        {_shipEngineShippingCarriers.WayFairFedEx , ShipperApi.ShipEngine }
                    };
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = false,
                        insuredValue = 0.00m,
                        provider = OrderInsuranceOptions.Provider.none
                    };
                    break;
                }
            //Channel Advisor and Veer Decor DesignsDirect Account
            case 1026039 or 1037060:
                {
                    _orderShippingInfo = new OrderShippingInfo()
                    {
                        name = "Designs Direct",
                        company = "Designs Direct",
                        street1 = "605 Philadelphia St",
                        street2 = "",
                        street3 = "",
                        city = "Covington",
                        state = "KY",
                        postalCode = "41011",
                        country = "US",
                        phone = "859-431-0290",
                        residential = false
                    };
                    //_appliedAccountNumbers = new List<string>() { "346593300" };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>()
                    {
                        //Designs Direct UPS
                        {_shipEngineShippingCarriers.DesignsDirectUPS , ShipperApi.ShipEngine }
                    };
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = false,
                        insuredValue = 0.00m,
                        provider = OrderInsuranceOptions.Provider.none
                    };
                    break;
                }
            //Basic Stores
            default:
                {

                    _orderShippingInfo = _defaultShippingInfo;
                    _insuranceOptions = new OrderInsuranceOptions()
                    {
                        insureShipment = false,
                        insuredValue = 0.00m,
                        provider = OrderInsuranceOptions.Provider.none
                    };
                    _appliedShipperIds = new Dictionary<string, ShipperApi>() {
                                    {_shipEngineShippingCarriers.UPSGroundPlus, ShipperApi.ShipEngine  },
                                    {_shipEngineShippingCarriers.UPSGroundSurePost, ShipperApi.ShipEngine },
                                    {_shipEngineShippingCarriers.StampsUSPS , ShipperApi.ShipEngine },
                                    {_shipEngineShippingCarriers.InternalFedEx , ShipperApi.ShipEngine },
                                    {_shipEngineShippingCarriers.DHL , ShipperApi.ShipEngine },
                                    {_shipStationShippingCarriers.StampsUSPS , ShipperApi.ShipStation },
                                    {_shipStationShippingCarriers.UPSGroundSurePost , ShipperApi.ShipStation },
                                    {_shipStationShippingCarriers.UPSGroundPlus , ShipperApi.ShipStation },
                                    {_shipStationShippingCarriers.InternalFedEx , ShipperApi.ShipStation }
                                };
                    break;
                }
        }
        //Rule Overrides
        _insuranceOptions = new OrderInsuranceOptions() //Request No more Shipsurance 
        {
            insureShipment = false,
            insuredValue = 0.00m,
            provider = OrderInsuranceOptions.Provider.none
        };
        return this;
    }
    public ICustomShippingBuilder SetDefaultShippingInfo()
    {
        _defaultShippingInfo = new OrderShippingInfo()
        {
            name = "Fulfillment Center",
            company = "Fulfillment Center",
            city = "Lafayette",
            country = "US",
            state = "LA",
            street1 = "1229 NW Evangeline Thruway",
            phone = "337-306-9349",
            postalCode = "70501",
            residential = false,
            street2 = "",
            street3 = ""
        };
        _orderShippingInfo = _defaultShippingInfo; // Ensure _orderShippingInfo is initialized
        return this;
    }
    /// <summary>
    /// Set a temporary hold on the service. This is used to prevent shipping during certain periods, such as holidays or special events.
    /// </summary>
    /// <returns></returns>
    public ICustomShippingBuilder SetTemporaryHoldOnService()
    {
        _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
        _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
        _appliedShipperIds.Remove(_shipEngineShippingCarriers.DHL);
        return this;
    }

    public ICustomShippingBuilder UpdateValidShipperIdsByWeight(OrderWeight weight)
    {
        if (weight == null)
        {
            throw new ArgumentNullException(nameof(weight), "Weight cannot be null.");
        }

        _appliedShipperIds ??= []; // Ensure initialization

        if (weight.value < 11 && weight.units == OrderWeight.Units.pounds
            || weight.value < 176 && weight.units == OrderWeight.Units.ounces)
        {
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundPlus);
        }
        else
        {
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundSurePost);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundSurePost);
        }
        if (weight.value > 5 && weight.units == OrderWeight.Units.pounds
            || weight.value > 80 && weight.units == OrderWeight.Units.ounces)
        {
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
        }

        return this;
    }

    public ICustomShippingBuilder UpdateValidShipperIdsByDimensions(OrderDimensions dimensions)
    {
        if (dimensions.length > 17 && dimensions.width > 17 && dimensions.units == OrderDimensions.Units.inches
                        || (dimensions.length > 22 || dimensions.width > 22) && dimensions.units == OrderDimensions.Units.inches)
        {
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.DHL);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
        }
        else if (dimensions.length * dimensions.height * dimensions.width > 1728 && dimensions.units == OrderDimensions.Units.inches)
        {
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.DHL);
        }
        return this;
    }

    public ICustomShippingBuilder UpdateValidShipperIdsByShippingInfo(OrderShippingInfo destination)
    {
        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination), "Destination cannot be null.");
        }

        if (string.IsNullOrEmpty(destination.country) || string.IsNullOrEmpty(destination.state) || string.IsNullOrEmpty(destination.street1))
        {
            throw new ArgumentException("Destination properties cannot be null or empty.");
        }

        // Eliminate Non-International Options
        if (destination.country.ToLowerInvariant() is not "us"
            || destination.state.ToLower() is "ak" or "hi"
            || destination.state.ToLower() is "aa" or "ap" or "ae"
            || destination.city?.ToLower() is "apo" or "fpo" or "dpo"
            || _territoryAndIslandCodes.Contains(destination.state.ToLower())
            || destination.street1.ToUpper().Contains("PO "))
        {
            _appliedShipperIds?.Remove(_shipEngineShippingCarriers.OSM);
            _appliedShipperIds?.Remove(_shipStationShippingCarriers.OSM);
            _appliedShipperIds?.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
        }

        if (destination.country.ToLower() is "ca")
        {
            _appliedShipperIds?.Remove(_shipEngineShippingCarriers.StampsUSPS);
            _appliedShipperIds?.Remove(_shipStationShippingCarriers.StampsUSPS);
        }

        return this;
    }

    public ICustomShippingBuilder UpdateValidShipperIdsByItems(List<OrderItem> orderItems)
    {
        foreach (var item in orderItems)
        {
            if (item.sku == null || item.ERPProductId == null && item.ERPBundleId == null)
            { continue; }
            if (item.Product != null && item.Product.Departments != null)
            {
                if (item.sku.ToUpper().StartsWith("PLT") || item.Product.Departments.Any(x => x.DepartmentName.Contains("Plants")))
                {
                    _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
                    _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
                    _appliedShipperIds.Remove(_shipEngineShippingCarriers.StampsUSPS);
                    _appliedShipperIds.Remove(_shipStationShippingCarriers.StampsUSPS);
                }
            }
            //Eliminate Non-Plant shippable and MAT
            if (item.sku.ToUpper().StartsWith("MAT"))
            {
                //Eliminate OSM
                _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
                _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);

                break;
            }
            if (item.sku.ToUpper().EndsWith("EXP"))
            {
                _expedited = true;
                _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
                _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
                _appliedShipperIds.Remove(_shipEngineShippingCarriers.DHL);
                _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
                _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundPlus);
            }
        }
        return this;
    }

    public ICustomShippingBuilder UpdateValidShipperIdsByOrder(Order order)
    {
        if (order.orderNumber.ToUpper().EndsWith("EXP"))
        {
            _expedited = true;
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.StampsUSPS);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.StampsUSPS);
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.OSM);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.OSM);
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.DHL);
            _appliedShipperIds.Remove(_shipEngineShippingCarriers.UPSGroundPlus);
            _appliedShipperIds.Remove(_shipStationShippingCarriers.UPSGroundPlus);
        }
        if (_allowedPreferredService && order.requestedShippingService != null)
        {
            switch (order.requestedShippingService!.ToLowerInvariant())
            {
                case "usps priority (2-3 day)":
                    {
                        _setPreferredServiceCode = "usps_priority_mail";
                        break;
                    }
                case "ups 2-day" or "2 day shipping" or "express 2 day delivery":
                    {
                        _setPreferredServiceCode = "ups_2nd_day_air";
                        break;
                    }
                case "ups overnight":
                    {
                        _setPreferredServiceCode = "ups_next_day_air";
                        break;
                    }
                case "standard":
                default:
                    {
                        //If they set their preferred method as "Standard" or anything not mapped above. Then ship as normal.
                        _allowedPreferredService = false;
                        break;
                    }
            }
        }
        else
        {
            _allowedPreferredService = false;
        }
        return this;
    }

    public ICustomShippingBuilder RemoveInvalidShippingServicesByShippingEstimates(List<ShipEngineShippingEstimate> shipEngineShippingEstimates)
    {
        //Shipping estimates case is agnostic to any services already removed so
        //Methods here are built by eliminating any services by a carrier at a time.
        //The first check should always be declaring which carrier's services are being removed.

        //If the store allows for preferred services then screw the system.
        if (_allowedPreferredService)
        {
            shipEngineShippingEstimates.RemoveAll(x => x.service_code != _setPreferredServiceCode);
            return this;
        }

        //Endicia/OSM services have other service info and need to be eliminated
        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.OSM && !x.service_code.Contains("osm")); //our account
        //Remove NonAvailable USPS Services
        shipEngineShippingEstimates //our account 
            .RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.StampsUSPS && x.service_code != "usps_first_class_mail" &&
        x.service_code != "usps_priority_mail" && x.service_code != "usps_ground_advantage" &&
        !x.service_code.Contains("international") ||
        x.package_type != "package" && x.carrier_id == _shipEngineShippingCarriers.StampsUSPS);
        //Ensure Expedited Services
        if (_expedited)
        {
            shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.StampsUSPS && x.service_code != "usps_priority_mail");
        }

        //Remove NonAvailable UPS Services
        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.UPSGroundPlus && x.service_code != "ups_ground");//11lb+ account
        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.UPSGroundSurePost && x.service_code != "ups_ground");//reg account
        //Remove NonAvailable Fedex Services
        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.InternalFedEx && //our account
        x.service_code != "fedex_2day" &&
        x.service_code != "fedex_home_delivery" &&
        x.service_code != "fedex_smartpost_parcel_select" &&
        x.service_code != "fedex_standard_overnight" &&
        x.service_code != "fedex_ground_international");

        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.KirklandFedEx && //Kirkland's Fedex
        x.service_code != "fedex_ground" 
        //&& x.service_code != "fedex_2day" 
        //&& x.service_code != "fedex_standard_overnight"
        );

        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.WayFairFedEx && //WayFair's Fedex
        x.service_code != "fedex_ground" &&
        x.service_code != "fedex_home_delivery" &&
        x.service_code != "fedex_2day" &&
        x.service_code != "fedex_standard_overnight");


        //Remove NonAvailable DHL Services
        shipEngineShippingEstimates.RemoveAll(x => x.carrier_id == _shipEngineShippingCarriers.DHL &&
        x.service_code != "smartmail_parcels_ground" &&
        x.service_code != "smartmail_parcel_plus_ground" &&
        x.service_code != "dhl_parcel_international_direct_priority" &&
        x.service_code != "dhl_parcel_international_direct_standard");//our account

        return this;
    }
    public ICustomShippingBuilder AddEnsuredValidServiceCodesByShippingEstimates(List<ShipEngineShippingEstimate> shipEngineShippingEstimates)
    {
        if (_storeId == 1002827)
        {
            shipEngineShippingEstimates.Add(new ShipEngineShippingEstimate() { carrier_id = _shipEngineShippingCarriers.WayFairFedEx, service_code = "fedex_ground", service_type = "Fedex Ground", carrier_code = "fedex", carrier_nickname = "Wayfair DD", carrier_friendly_name = "FedEx", trackable = true, rate_type = "fedex_ground", package_type = "package", delivery_days = 3, guaranteed_service = false, negotiated_rate = false, shipping_amount = new ShipEngineShippingEstimate.Amount() { currency = "USD", amount = 1.00m }, estimated_delivery_date = DateTime.Now.AddDays(3), ship_date = DateTime.Now, validation_status = "valid" });
        }
        else if (_storeId == 1001347)
        {
            shipEngineShippingEstimates.Add(new ShipEngineShippingEstimate() { carrier_id = _shipEngineShippingCarriers.KirklandFedEx, service_code = "fedex_ground", service_type = "Fedex Ground", carrier_code = "fedex", carrier_nickname = "Kirkland's Fedex", carrier_friendly_name = "FedEx", trackable = true, rate_type = "fedex_ground", package_type = "package", delivery_days = 3, guaranteed_service = false, negotiated_rate = false, shipping_amount = new ShipEngineShippingEstimate.Amount() { currency = "USD", amount = 1.00m }, estimated_delivery_date = DateTime.Now.AddDays(3), ship_date = DateTime.Now, validation_status = "valid" });
        }
        return this;
    }

    public CustomShipping Build()
    {
        return new CustomShipping(this);
    }

}
public class CustomShippingDirector
{
    public CustomShipping Construct(ICustomShippingBuilder builder, Order order)
    {
        if (order.shipTo == null || string.IsNullOrEmpty(order.shipTo.country) || string.IsNullOrEmpty(order.shipTo.state) || string.IsNullOrEmpty(order.shipTo.street1))
        {
            throw new ArgumentException("Order's shipTo property must be properly initialized.");
        }

        builder = builder
            .SetDefaultShippingInfo()
            .SetStoreId(order.advancedOptions.storeId)
            .SetTemporaryHoldOnService()
            .UpdateValidShipperIdsByOrder(order); //Placed here for possible change in AllowedPreferredServiceCode property
        if (!builder.AllowedPreferredServiceCode)
        {
            builder = builder.UpdateValidShipperIdsByWeight(order.weight).UpdateValidShipperIdsByDimensions(order.dimensions);
        }
        return builder
            .UpdateValidShipperIdsByShippingInfo(order.shipTo)
            .UpdateValidShipperIdsByItems(order.items)
            .Build();
    }
}

public class CustomShipping
{
    // Properties  
    public long StoreId { get; private set; }
    public OrderShippingInfo OrderShippingInfo { get; private set; }
    public OrderInsuranceOptions InsuranceOptions { get; private set; }
    public Dictionary<string, ShipperApi> AppliedShipperIds { get; private set; }

    public CustomShipping(ICustomShippingBuilder builder)
    {
        // Set properties based on builder values  
        StoreId = builder.StoreId;
        OrderShippingInfo = builder.OrderShippingInfo;
        InsuranceOptions = builder.InsuranceOptions;
        AppliedShipperIds = builder.AppliedShipperIds;

    }

    public enum ShipperApi
    {
        None,
        ShipEngine,
        ShipStation
    }
}
public class ShipEngineShippingEstimate
{
    public string rate_type { get; set; }
    public string carrier_id { get; set; }
    public Amount? shipping_amount { get; set; }
    public Amount? insurance_amount { get; set; }
    public Amount? confirmation_amount { get; set; }
    public Amount? other_amount { get; set; }
    public int? zone { get; set; }
    public string package_type { get; set; }
    public int? delivery_days { get; set; }
    public bool guaranteed_service { get; set; }
    public DateTime? estimated_delivery_date { get; set; }
    public string carrier_delivery_days { get; set; }
    public DateTime? ship_date { get; set; }
    public bool negotiated_rate { get; set; }
    public string service_type { get; set; }
    public string service_code { get; set; }
    public bool trackable { get; set; }
    public string carrier_code { get; set; }
    public string carrier_nickname { get; set; }
    public string carrier_friendly_name { get; set; }
    public string validation_status { get; set; }
    public string[] warning_messages { get; set; }
    public string[] error_messages { get; set; }
    public struct Amount
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }
}
public class OSMShippingEstimate
{
    public string Session { get; set; }
    public int CostCenterId { get; set; }

    public OSMPackageContainer Packages { get; set; }

    public class OSMPackageContainer
    {
        public OSMPackage Package { get; set; }
    }
    public class OSMPackage
    {
        public int PkgNumber { get; set; }
        public string Reference1 { get; set; }
        public decimal RatedWeight { get; set; }
        public float DimWeightUsed { get; set; }
        public string MailClass { get; set; }
        public decimal? EstimateAmount { get; set; }
        public decimal? DasCharge { get; set; }
        public decimal? FuelCharge { get; set; }
        public decimal? MiscCharge { get; set; }
        public decimal? TotalEstimate { get; set; }
        public string OutOfNetworkWhiteZone { get; set; }
        public string CustomsInfoRequired { get; set; }
        public string Error { get; set; }
    }
}
