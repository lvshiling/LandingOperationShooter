#import "CountryTool.h"

/*

@interface CountryTool() {
    CLLocationManager *locationManager;
    CLLocation *location;
    @public NSString *country;
    @public double latitude;
    @public double longitude;
    @public bool isInited;
    @public bool isAccessRequest;
    @public bool isCountryRequest;
    
}
    
    @end

@implementation CountryTool
    
+ (CountryTool *)pluginSharedInstance {
    static CountryTool *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[CountryTool alloc] init];
    });
    return sharedInstance;
}
    // kCLAuthorizationStatusNotDetermined = 0
    // kCLAuthorizationStatusRestricted = 1
    // kCLAuthorizationStatusDenied = 2
    // kCLAuthorizationStatusAuthorizedAlways = 3
    // kCLAuthorizationStatusAuthorizedWhenInUse = 4
-(int) getAuthrizationLevelForApplication {
    CLAuthorizationStatus authorizationStatus = [CLLocationManager authorizationStatus];
    return authorizationStatus;
}
    
-(void) requestAuthorizedWhenInUse {
    [self initializeLocalManager];
    [locationManager requestWhenInUseAuthorization];
}
    
-(void) initializeLocalManager {
    if (locationManager == nil) {
        locationManager = [[CLLocationManager alloc] init];
    }
    if (!isInited) {
        int authorizationLevel = [self getAuthrizationLevelForApplication];
        //NSLog(@"CountryTool authorizationLevel %d", authorizationLevel);
        if (authorizationLevel == kCLAuthorizationStatusNotDetermined) {
            // request
            if (!isAccessRequest){
                isAccessRequest = true;
                //NSLog(@"CountryTool request location access");
                [self requestAuthorizedWhenInUse];
            }
        }
        else if (authorizationLevel == kCLAuthorizationStatusAuthorizedAlways || authorizationLevel == kCLAuthorizationStatusAuthorizedWhenInUse) {
            // get location
            if (!isCountryRequest){
                isCountryRequest = true;
                location = locationManager.location;
                longitude = location.coordinate.longitude;
                latitude = location.coordinate.latitude;
                //NSLog(@"CountryTool longitude %.3f latitude %.3f", longitude, latitude);
                [self getAddressForLocation: location];
            }
        }
        else if (authorizationLevel == kCLAuthorizationStatusRestricted || authorizationLevel == kCLAuthorizationStatusDenied) {
            //NSLog(@"CountryTool location denied");
            // location denied, get country from locale
            [self getCountryFromLocale];
        }
    }
}
    
-(void)getAddressForLocation:(CLLocation *)locationForAddress {
    CLGeocoder *geocoder = [[CLGeocoder alloc] init];
    [geocoder reverseGeocodeLocation:locationForAddress completionHandler:^(NSArray *placemarks, NSError *error) {
        CountryTool *countryTool = [CountryTool pluginSharedInstance];
        if (error == nil && [placemarks count] > 0) {
            CLPlacemark *placemark = [placemarks lastObject];
            countryTool->country = placemark.ISOcountryCode;
            //NSLog(@"CountryTool country from location = %@", countryTool->country);
            isInited = true;
        } else {
            // get country from locale
            [self getCountryFromLocale];
        }
    } ];
}

-(void) getCountryFromLocale {
    isInited = true;
    NSLocale *locale = [NSLocale currentLocale];
    country = [locale objectForKey: NSLocaleCountryCode];
    //NSLog(@"CountryTool getCountryFromLocale country = %@", country);
}
    @end

*/
char * cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

extern "C" {
    void InitLocationIOS() {
        //CountryTool *countryTool = [CountryTool pluginSharedInstance];
        //[countryTool initializeLocalManager];
    }
    
    char * GetCountryIOS() {
        //CountryTool *countryTool = [CountryTool pluginSharedInstance];
        //return cStringCopy([countryTool->country cStringUsingEncoding:NSASCIIStringEncoding]);
        NSString *country;
        NSLocale *locale = [NSLocale currentLocale];
        country = [locale objectForKey: NSLocaleCountryCode];
        return cStringCopy([country cStringUsingEncoding:NSASCIIStringEncoding]);
    }
    
    double GetLatitudeIOS() {
        //CountryTool *countryTool = [CountryTool pluginSharedInstance];
        //return countryTool->latitude;
        return 0.0;
    }
    
    double GetLongitudeIOS() {
        //CountryTool *countryTool = [CountryTool pluginSharedInstance];
        //return countryTool->longitude;
        return 0.0;
    }
    
    bool IsInitedIOS(){
        //CountryTool *countryTool = [CountryTool pluginSharedInstance];
        //return countryTool->isInited;
        return true;
    }
}



