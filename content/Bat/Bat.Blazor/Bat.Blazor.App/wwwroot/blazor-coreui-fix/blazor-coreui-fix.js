/**
 * This script is used to fix a bug where CoreUI is not properly initialized with Blazor.
 */

(function () {
    const STORE_KEY_THEME = 'coreui-free-bootstrap-admin-template-theme';
    const getStoredTheme = () => localStorage.getItem(STORE_KEY_THEME);
    const setStoredTheme = theme => localStorage.setItem(STORE_KEY_THEME, theme);
    const getPreferredTheme = () => {
        const storedTheme = getStoredTheme();
        if (storedTheme) {
            return storedTheme;
        }
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    };
    const setTheme = theme => {
        if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            document.documentElement.setAttribute('data-coreui-theme', 'dark');
        } else {
            document.documentElement.setAttribute('data-coreui-theme', theme);
        }
        const event = new Event('ColorSchemeChange');
        document.documentElement.dispatchEvent(event);
    };
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        const storedTheme = getStoredTheme();
        if (storedTheme !== 'light' || storedTheme !== 'dark') {
            setTheme(getPreferredTheme());
        }
    });

    const PREFFERED_THEME = getPreferredTheme();
    setTheme(PREFFERED_THEME);

    let _CoreUIFixSidebarSleep = 100
    let _CoreUIFixSidebarCounter = 0
    function CoreUIFixSidebar() {
        // fix the sidar toggler
        if (typeof (coreui) === 'undefined' || typeof (coreui.Sidebar) === 'undefined' || typeof (document.querySelector) === 'undefined') {
            if (_CoreUIFixSidebarCounter++ < 5) {
                console.log(`[DEBUG] CoreUIFixSidebar: coreui/coreui.Sidebar/document.querySelector is undefined, retrying in ${_CoreUIFixSidebarSleep}ms...`)
                setTimeout(CoreUIFixSidebar, _CoreUIFixSidebarSleep);
                _CoreUIFixSidebarSleep *= 2
            }
            return;
        }

        console.log('[DEBUG] CoreUIFixSidebar: coreui/coreui.Sidebar/document.querySelector defined, initializing sidebar...')
        let element = document.querySelector('#sidebar');
        if (coreui && coreui.Sidebar && !coreui.Sidebar.getInstance(element)) {
            coreui.Sidebar.getOrCreateInstance(element);
        }
    }
    CoreUIFixSidebar();

    let _CoreUIFixThemeSwitcherSleep = 100
    let _CoreUIFixThemeSwitcherCounter = 0
    function CoreUIFixThemeSwitcher(prefferedTheme, fnSetStoredTheme, fnSetTheme) {
        // fix the theme switcher
        if (typeof (document.querySelector) === 'undefined' || typeof (document.querySelectorAll) === 'undefined' ) {
            if (_CoreUIFixThemeSwitcherCounter++ < 5) {
                console.log(`[DEBUG] CoreUIFixThemeSwitcher: document{querySelector/querySelectorAll} is undefined, retrying in ${_CoreUIFixThemeSwitcherSleep}ms...`)
                setTimeout(() => { CoreUIFixThemeSwitcher(prefferedTheme, fnSetStoredTheme, fnSetTheme); }, _CoreUIFixThemeSwitcherSleep);
                _CoreUIFixThemeSwitcherSleep *= 2
            }
            return;
        }

        console.log('[DEBUG] CoreUIFixThemeSwitcher: document{querySelector/querySelectorAll} defined, initializing theme switcher...')
        const showActiveTheme = theme => {
            const activeThemeIcon = document.querySelector('.theme-icon-active use');
            const btnToActive = document.querySelector(`[data-coreui-theme-value="${theme}"]`);
            if ( (!btnToActive || !btnToActive.querySelector) && !activeThemeIcon ) {
                return;
            }

            for (const element of document.querySelectorAll('[data-coreui-theme-value]')) {
                element.classList.remove('active');
                if (typeof(element.checked) === 'boolean') {
                    element.checked = false;
                }
            }
            if (btnToActive && btnToActive.querySelector) {
                btnToActive.classList.add('active');
                if (typeof(btnToActive.checked) === 'boolean') {
                    btnToActive.checked = true;
                }
            }

            if (activeThemeIcon && btnToActive && btnToActive.querySelector) {
                const svgOfActiveBtn = btnToActive.querySelector('svg use').getAttribute('xlink:href');
                activeThemeIcon.setAttribute('xlink:href', svgOfActiveBtn);
            }
        };

        showActiveTheme(prefferedTheme);
        for (const toggle of document.querySelectorAll('[data-coreui-theme-value]')) {
            toggle.addEventListener('click', () => {
                const theme = toggle.getAttribute('data-coreui-theme-value');
                fnSetStoredTheme(theme);
                fnSetTheme(theme);
                showActiveTheme(theme);
            });
        }
    }
    CoreUIFixThemeSwitcher(PREFFERED_THEME, setStoredTheme, setTheme);
})();
