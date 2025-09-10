//async function collectLoginData() {
//    const userAgent = navigator.userAgent;
//    const screenWidth = window.screen.width;
//    const screenHeight = window.screen.height;
//    const ipAddress = await getIPAddress();

//    const loginData = {
//        IPAddress: ipAddress,
//        Browser: getBrowserInfo(userAgent).name,
//        BrowserVersion: getBrowserInfo(userAgent).version,
//        OS: getOSInfo(userAgent),
//        ScreenWidth: screenWidth,
//        ScreenHeight: screenHeight,
//        Timestamp: new Date().toISOString()
//    };

//    console.log("Login Data:", loginData);

//    // Send data to server
//    sendClientData('/api/login-data', loginData);
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

//function getBrowserInfo(userAgent) {
//    const browsers = [
//        { name: "Edge", regex: /Edg\/(\d+(\.\d+)?)/ },
//        { name: "Chrome", regex: /Chrome\/(\d+(\.\d+)?)/ },
//        { name: "Firefox", regex: /Firefox\/(\d+(\.\d+)?)/ },
//        { name: "Safari", regex: /Safari\/(\d+(\.\d+)?)/ }
//    ];

//    for (const browser of browsers) {
//        const match = userAgent.match(browser.regex);
//        if (match) {
//            return { name: browser.name, version: match[1] };
//        }
//    }

//    return { name: "Unknown", version: "0" };
//}

//function getOSInfo(userAgent) {
//    const platforms = [
//        { name: "Windows", regex: /Windows NT (\d+\.\d+)/ },
//        { name: "Mac OS", regex: /Mac OS X (\d+[\._]\d+)/ },
//        { name: "Linux", regex: /Linux/ },
//        { name: "Android", regex: /Android (\d+\.\d+)/ },
//        { name: "iOS", regex: /iPhone|iPad/ }
//    ];

//    for (const platform of platforms) {
//        const match = userAgent.match(platform.regex);
//        if (match) {
//            return `${platform.name} ${match[1] || ""}`;
//        }
//    }

//    return "Unknown OS";
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

//document.addEventListener("DOMContentLoaded", collectLoginData);