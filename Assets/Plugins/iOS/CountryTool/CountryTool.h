// Commented operations with location
/*
#import <CoreLocation/CoreLocation.h>

@interface CountryTool : NSObject

@end
*/

extern "C" void InitLocationIOS();

extern "C" char * GetCountryIOS();

extern "C" double GetLatitudeIOS();

extern "C" double GetLongitudeIOS();

extern "C" bool IsInitedIOS();

