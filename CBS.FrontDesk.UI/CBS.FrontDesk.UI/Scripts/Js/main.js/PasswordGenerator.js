function generateStrongPassword() {
    const specials = "!@#$%^&*()_+[]{}|~?=";
    const forbidden = ["password", "admin", "welcome"];
    const evenYears = Array.from({ length: new Date().getFullYear() - 1899 + 2 }, (_, i) => 1900 + i).filter(y => y % 2 === 0).map(String);

    const wordBank = [
        "Sun", "River", "Cloud", "Tree", "Moon", "Star", "Sky", "Ocean", "Stone", "Light",
        "Fire", "Lemon", "Blue", "Train", "Fox", "Rain", "Gold", "Mint", "Wave", "Echo",
        "Pie", "Forest", "Shadow", "Dream", "Sound", "Flame", "Wind", "Dust", "Rock", "Leaf",
        "Snow", "Wolf", "Bear", "Lion", "Tiger", "Falcon", "Eagle", "Owl", "Ice", "Storm",
        "Berry", "Frost", "Spring", "Autumn", "Summer", "Winter", "Dawn", "Dusk", "Hope", "Peace",
        "Grace", "Truth", "Love", "Honor", "Faith", "Glory", "Spark", "Shine", "Glow", "Ash",
        "Blaze", "Bolt", "Thunder", "Steel", "Iron", "Copper", "Bronze", "Crystal", "Jade", "Amber",
        "Ruby", "Sapphire", "Emerald", "Pearl", "Topaz", "Quartz", "Diamond", "Opal", "Coral", "Stone",
        "Shadow", "Mirror", "Light", "Dark", "Bright", "Silent", "Loud", "Swift", "Calm", "Brave",
        "Strong", "Clever", "Wise", "Kind", "Pure", "Wild", "Gentle", "Bold", "Sharp", "Cool",
        "Neon", "Violet", "Lilac", "Cyan", "Indigo", "Azure", "Teal", "Crimson", "Maroon", "Navy",
        "Slate", "Ivory", "Charcoal", "Amber", "Plum", "Rose", "Olive", "Pear", "Cocoa", "Vanilla",
        "Bubble", "Jelly", "Candy", "Sugar", "Honey", "Sweet", "Bitter", "Salt", "Spice", "Chili",
        "Curry", "Mango", "Melon", "Coconut", "Lime", "Orange", "Peach", "Grape", "Berry", "Fig",
        "Date", "Papaya", "Guava", "Banana", "Apple", "Cherry", "Pine", "Bamboo", "Moss", "Fern",
        "Willow", "Cedar", "Elm", "Oak", "Maple", "Ash", "Hazel", "Ivy", "Thorn", "Root",
        "Drift", "Tide", "Wave", "Foam", "Stream", "Creek", "Brook", "Bay", "Lagoon", "Harbor",
        "Island", "Shore", "Beach", "Cliff", "Cave", "Hill", "Valley", "Ridge", "Peak", "Summit"
    ];

    const upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const lower = "abcdefghijklmnopqrstuvwxyz";
    const digits = "0123456789";
    const all = upper + lower + digits + specials;

    function getRandom(arr) {
        return arr[Math.floor(Math.random() * arr.length)];
    }

    function shuffle(str) {
        return str.split('').sort(() => 0.5 - Math.random()).join('');
    }

    function hasSequentialPattern(str) {
        const sequential = ["1234", "2345", "3456", "abcd", "bcde", "cdef", "qwer"];
        return sequential.some(seq => str.toLowerCase().includes(seq));
    }

    function hasRepeatedChars(str) {
        return /(.)\1{2,}/.test(str);
    }

    function hasEvenYear(str) {
        return evenYears.some(year => str.includes(year));
    }

    function hasForbiddenWord(str) {
        return forbidden.some(word => str.toLowerCase().includes(word));
    }

    function hasConsecutiveSpecials(str) {
        for (let i = 1; i < str.length; i++) {
            if (specials.includes(str[i]) && specials.includes(str[i - 1])) {
                return true;
            }
        }
        return false;
    }

    function hasRepeatedSpecials(str) {
        const seen = new Set();
        for (let ch of str) {
            if (specials.includes(ch)) {
                if (seen.has(ch)) return true;
                seen.add(ch);
            }
        }
        return false;
    }

    function isValid(pwd) {
        const specialCount = new Set(pwd.split('').filter(c => specials.includes(c))).size;
        return (
            pwd.length >= 12 &&
            /[A-Z]/.test(pwd) &&
            /[a-z]/.test(pwd) &&
            /\d/.test(pwd) &&
            specialCount >= 4 &&
            !hasRepeatedSpecials(pwd) &&
            !hasConsecutiveSpecials(pwd) &&
            !hasForbiddenWord(pwd) &&
            !hasEvenYear(pwd) &&
            !hasSequentialPattern(pwd) &&
            !hasRepeatedChars(pwd)
        );
    }

    let password = "";
    let attempts = 0;

    while (attempts < 1000) {
        const words = [getRandom(wordBank), getRandom(wordBank), getRandom(wordBank)];
        const digitsPart = getRandom(digits) + getRandom(digits);
        const specialChars = [...specials].sort(() => 0.5 - Math.random()).slice(0, 4);

        let base = [];
        base.push(...words);
        base.push(...specialChars);
        base.push(digitsPart);

        password = shuffle(base.join(''));

        if (isValid(password)) break;
        attempts++;
    }

    document.getElementById("generatedPassword").value = password || "Unable to generate a valid password.";
    document.getElementById("copyFeedback").classList.add("d-none");
}

function copyGeneratedPassword() {
    const input = document.getElementById("generatedPassword");
    input.select();
    input.setSelectionRange(0, 99999);
    document.execCommand("copy");

    const feedback = document.getElementById("copyFeedback");
    feedback.classList.remove("d-none");
    setTimeout(() => feedback.classList.add("d-none"), 2500);
}

function downloadPassword() {
    const password = document.getElementById("generatedPassword").value;
    const username = document.getElementById("currentUsername")?.value || "N/A";
    const domain = window.location.origin;

    if (!password || password === "Unable to generate a valid password.") {
        alert("No valid password to download.");
        return;
    }

    const timestamp = new Date().toLocaleString();
    const content = `
                    ============================================
                              TRUST SOFT CREDIT (TSC)
                            🔐 Secure Password Confirmation
                    ============================================

                    📅 Generated on : ${timestamp}
                    👤 Username     : ${username}
                    🔐 Password     : ${password}
                    🌐 Login URL    : ${domain}

                    🔐 Your new system-generated password:
                    --------------------------------------------
                    ${password}
                    --------------------------------------------

                    ⚠️ IMPORTANT:
                    Please store this password securely.
                    Do NOT share it via email, chat, or written notes.

                    📌 RECOMMENDED ACTIONS:
                    - Access your account at: ${domain}
                    - Change this password following password policy durations
                    - Save it in a secure password manager (not your browser)

                    © ${new Date().getFullYear()} Trust Soft Credit. All rights reserved.
                    `;

    const blob = new Blob([content], { type: "text/plain" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = `TSC_Generated_Password_${username}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}
