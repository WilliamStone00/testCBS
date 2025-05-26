//async function collectClientData() {
//    const userAgent = navigator.userAgent;
//    const screenWidth = window.screen.width;
//    const screenHeight = window.screen.height;
//    const ipAddress = await getIPAddress();
//    const location = await getLocation(ipAddress);

//    const clientData = {
//        IPAddress: ipAddress,
//        Browser: getBrowserInfo(userAgent).name,
//        BrowserVersion: getBrowserInfo(userAgent).version,
//        OS: getOSInfo(userAgent),
//        ScreenWidth: screenWidth,
//        ScreenHeight: screenHeight,
//        Location: location,
//        Timestamp: new Date().toISOString()
//    };

//    console.log("Client Data:", clientData);

//    // Send data to server
//    sendClientData('/api/client-data', clientData);
//}

//async function getIPAddress() {
//    try {
//        const response = await fetch('https://api64.ipify.org?format=json');
//        const data = await response.json();
//        return data.ip;
//    } catch (error) {
//        console.error("Error fetching IP:", error);
//        return "Unknown IP";
//    }
//}

//async function getLocation(ipAddress) {
//    try {
//        const response = await fetch(`https://ipinfo.io/${ipAddress}/json`);
//        const data = await response.json();
//        return `${data.city}, ${data.region}, ${data.country}`;
//    } catch (error) {
//        console.error("Error fetching location:", error);
//        return "Unknown Location";
//    }
//}

//async function sendClientData(endpoint, data) {
//    try {
//        await fetch(endpoint, {
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json'
//            },
//            body: JSON.stringify(data)
//        });
//    } catch (error) {
//        console.error("Error sending data:", error);
//    }
//}

//document.addEventListener("DOMContentLoaded", collectClientData);