using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebView2CustomTitleBarApp
{
    public partial class Form1 : Form
    {
        private bool isFullscreen = false;
        private System.Windows.Forms.Timer hoverTimer;
        private bool isTitleBarVisible = true;
        private Panel titleBarPanel;
        private WebView2 webView;
        private bool isDragging = false;
        private Point offset;
        private bool isResizing = false;
        private Point resizeOffset;
        private Label resizeHandle;

        private string userScript = @"
        (() => {
            ""use strict"";
 
            const hideAdsDiv = () => {
                const adsDiv = document.querySelector('.ads_ads__Z1cPk');
                if (adsDiv) {
                    adsDiv.style.display = 'none';
                }
            };
            hideAdsDiv();
            const observer = new MutationObserver(hideAdsDiv);
            observer.observe(document.body, { childList: true, subtree: true });
 
            const script = document.createElement('script');
            script.src = 'https://kit.fontawesome.com/d8b6e64f10.js';
            script.crossOrigin = 'anonymous';
            document.head.appendChild(script);
 
            const styles = `
                .luna-menu {
                    top: 10px;
                    left: 10px;
                    background: rgba(25, 29, 33, 1);
                    color: white;
                    padding: 0px;
                    font-size: 14px;
                    border-radius: 4px;
                    z-index: 5;
                    border: 1px solid #505050;
                    cursor: default;
                }
                .luna-menu.collapsed #main-menu {
                    display: none;
                }
                .luna-menu.collapsed .luna-menu_title {
                    border-bottom: none;
                }
                .luna-menu_title {
                    font-weight: bold;
                    padding: 4px 8px;
                    cursor: pointer;
                    margin-bottom: 0px;
                    background: rgba(116, 7, 0, 1);
                    border-top-left-radius: 4px;
                    border-top-right-radius: 4px;
                    border-bottom: 1px solid #505050;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                }
                .luna-menu_title:hover {
                    background-color: #a70a00;
                }
                .luna-menu_item {
                    margin: 5px 0;
                    padding: 3px;
                    cursor: pointer;
                    display: flex;
                    align-items: center;
                }
                .luna-menu_item:hover {
                    background-color: hsla(0, 0%, 100%, .1);
                    color: #f8ec94;
                }
                .luna-hide-scan_lines::after {
                    content: none !important;
                }
                .luna-checkbox {
                    appearance: none;
                    margin-right: 5px;
                    width: 20px;
                    height: 20px;
                    background-color: #303438;
                    border: 2px solid black;
                    border-radius: 3px;
                    position: relative;
                }
                .luna-checkbox:checked {
                    background-color: rgba(116, 7, 0, 1);
                }
                .luna-checkbox:checked::after {
                    content: '';
                    position: absolute;
                    left: 3px;
                    top: 3px;
                    width: 10px;
                    height: 10px;
                    background-color: #f8ec94;
                }
                 .luna-checkbox input:checked + .luna-checkbox::after {
                    display: block;
                }
                #toggle-icon {
                    transition: transform 0.5s, filter 0.9s;
                    --drop-shadow: drop-shadow(2px 3px 0 #000000);
                    filter: var(--drop-shadow);
                }
 
                .luna-menu_title svg {
                    margin-left: -3px;
                    color: #f8ec94;
                    filter: var(--drop-shadow);
                }
                .menu-title-text {
                    flex-grow: 1;
                    margin-left: 4px;
                    padding-left: 0px;
                }
                .modal {
                    display: none;
                    position: fixed;
                    z-index: 10001;
                    left: 50%;
                    top: 50%;
                    transform: translate(-50%, -50%);
                    background-color: rgba(25, 29, 33, 1);
                    border: 2px solid #505050;
                    border-radius: 10px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.5);
                }
                .modal.show {
                    display: block;    
                }       
                .modal #close-modal {
                    display: block;
                    margin: 20px auto 0 auto;
                }            
                #close-modal {
                    display: inline-block;
                    width: 100px;
                    padding: 10px 20px;
                    background-color: rgba(25, 29, 33, 1);
                    border: 1px solid #505050;
                    border-radius: 0;
                    cursor: pointer;
                    transition: color 0.3s, outline 0.3s;
                    box-sizing: border-box;
                }
 
                #close-modal:hover {
                    outline: 2px solid #f8ec94;
                    color: #f8ec94;
                }     
                .modal-title {
                    font-weight: bold;
                    padding: 4px 8px;
                    margin-bottom: 0px;
                    background: rgba(116, 7, 0, 1);
                    border-top-left-radius: 4px;
                    border-top-right-radius: 4px;
                    border-bottom: 1px solid #505050;
                }                 
                .custom-toast {
                    position: fixed;
                    bottom: 20px;
                    right: 20px;
                    background-color: rgba(25, 29, 33, 1);
                    color: #fff;
                    padding: 15px;
                    border-radius: 5px;
                    border: 4px solid #f8ec94;            
                    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                    z-index: 10000;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }
                .custom-toast-message {
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }
                .custom-toast-icon svg {
                    fill: #fff;
                }
                .custom-toast-close {
                    margin-left: auto;
                }
                .custom-close-button {
                    background: none;
                    border: none;
                    cursor: pointer;
                }
                .custom-close-button svg {
                    fill: #fff;
                }            
                #twitter-link button {
                    background-color: #1DA1F2;
                    width: 220px;
                    font-size: 8px;
                    padding: 8px 16px;            
                    margin-bottom: 5px;
                    background-color: rgba(25, 29, 33, 1);
                    border: 1px solid #505050;            
                    border-radius: 0;
                    cursor: pointer;
                    transition: color 0.3s, outline 0.3s;
                    box-sizing: border-box;
                }
                #twitter-link button:hover {
                    outline: 2px solid #f8ec94;
                    color: #f8ec94;
                }                
                .button-container {
                    display: flex;
                    align-items: center;
                    justify-content: center; 
                }     
                .change-key {
                    display: inline-block;
                    width: 250px;
                    font-size: 10px;
                    padding: 5px 10px;
                    background-color: rgba(25, 29, 33, 1);
                    border: 1px solid #505050;
                    border-radius: 0;
                    cursor: pointer;
                    transition: color 0.3s, outline 0.3s;
                    box-sizing: border-box;
                }
 
                .change-key:hover {
                    outline: 2px solid #f8ec94;
                    color: #f8ec94;
                }                   
            `;
 
 
            const styleSheet = document.createElement(""style"");
            styleSheet.type = ""text/css"";
            styleSheet.innerText = styles;
            document.head.appendChild(styleSheet);
 
 
            const menuOptions = [
                {
                    label: ' Enable Full-Screen',
                    description: 'This allows you to watch in full-screen mode. Each time you press the checkbox, it will enable full-screen mode.',
                    action: () => {
                        const videoElement = document.querySelector('video');
                        const fullscreenCheckbox = document.querySelector('#luna-checkbox-0'); 
                        if (videoElement) {
                            videoElement.requestFullscreen();
                        }
 
                        document.addEventListener('fullscreenchange', () => {
                            if (!document.fullscreenElement && fullscreenCheckbox) {
                                fullscreenCheckbox.checked = false;
                            }
                        });
                    }
                },
                {
                    label: ' Enable PiP',
                    description: 'This allows you to watch in Picture-in-Picture mode. Each time you press the checkbox, it will enable Picture-in-Picture mode. To exit PiP mode, press the checkbox again.',
                    action: () => {
                        const videoElement = document.querySelector('video');
                        const pipCheckbox = document.querySelector('#luna-checkbox-1');
                        if (videoElement && pipCheckbox) {
                            if (pipCheckbox.checked) {
                                videoElement.requestPictureInPicture();
                            } else {
                                document.exitPictureInPicture();
                            }
                        }
 
                        document.addEventListener('leavepictureinpicture', () => {
                            const pipCheckbox = document.querySelector('#luna-checkbox-1');
                            if (pipCheckbox) {
                                pipCheckbox.checked = false;
                            }
                        });
                    }
                },
                {
                    label: ' Hide Scan Lines',
                    description: 'This allows you to hide the scan lines. By default, the website has scan lines over all elements of the website to simulate the look of CRT.',
                    action: () => {
                        document.body.classList.toggle('luna-hide-scan_lines');
                        saveCheckboxState('luna-checkbox-2', document.body.classList.contains('luna-hide-scan_lines'));
                    }
                },
                {
                    label: ' Block All SFX',
                    description: 'This allows you to block all sound effects. This will block all sound effects from the website outside of the video. This includes TTS/SFX previews.',
                    action: () => {
                        interceptPlay();
                    }
                },
                {
                    label: ' Hide Clickable Zones',
                    description: 'This allows you to hide clickable zones. These are the red zones that appear when you hover over the video. These zones remain clickable even when hidden.',
                    action: () => {
                        const style = document.querySelector('#clickable-zones-style');
                        if (style) {
                            style.disabled = !style.disabled;
                        } else {
                            const newStyle = document.createElement('style');
                            newStyle.id = 'clickable-zones-style';
                            newStyle.innerHTML = `
                                .clickable-zones_clickable-zones__OgYjT polygon.clickable-zones_live-stream__i75zd:hover {
                                    fill: rgba(243, 14, 0, 0) !important;
                                }
                            `;
                            document.head.appendChild(newStyle);
                        }
                    }
                },
                {
                    label: 'Sidebar Cleanup',
                    description: 'This allows you to hide certain elements from the sidebar. This includes the item generator, inventory, missions, polls, and footer. Daily XP remains untouched.',
                    action: () => {
                        const style = document.querySelector('#block-elements-style');
                        if (style) {
                            style.disabled = !style.disabled;
                        } else {
                            const newStyle = document.createElement('style');
                            newStyle.id = 'block-elements-style';
                            newStyle.innerHTML = `
                                .item-generator_item-generator__TCQ9l,
                                .inventory_inventory__7bCIe,
                                .footer_footer__Mnt6p,
                                .missions_missions__haRAj,
                                .poll_poll__QyVsN {
                                    display: none !important;
                                }
                            `;
                            document.head.appendChild(newStyle);
                        }
                    }
                },
                {
                    label: 'Filter Toast Messages',
                    description: 'This allows you to filter out toast messages outside of admin messages. Any ""level up"", ""plushie"", or ""gifted"" messages will be hidden.',
                    action: () => {
                        const filterCheckbox = document.querySelector('#luna-checkbox-filter-toasts');
                        if (filterCheckbox) {
                            filterCheckbox.checked = !filterCheckbox.checked;
                            filterToasts();
                        }
                    }
                },     
                {
                    label: 'Block Global Missions',
                    description: 'This allows you to block the global missions from appearing. You WILL lose out on xp, but you will not have to deal with the popups.',
                    action: () => {
                        const checkbox = document.getElementById('luna-checkbox-7');
                        const labelElement = document.querySelector('label[for=""luna-checkbox-7""]');
        
                        const updateLabel = (showBell) => {
                            if (showBell) {
                                labelElement.innerHTML = 'Block Global Missions <i class=""fa-regular fa-bell"" style=""margin-left: 8px; color:#fd0f00;""></i>';
                            } else {
                                labelElement.innerHTML = 'Block Global Missions';
                            }
                        };
        
                        const checkModal = () => {
                            let showBell = false;
        
                            const modalBackdrops = document.querySelectorAll('.global-mission-modal_backdrop__oVezg');
                            modalBackdrops.forEach(modalBackdrop => {
                                modalBackdrop.style.setProperty('display', 'none', 'important');
                            });
        
                            const modalContainers = document.querySelectorAll('.modal_modal-container__iQODa, .modal_modal__MS70U');
                            modalContainers.forEach(modalContainer => {
                                if (modalContainer && modalContainer.textContent.includes('Global Mission')) {
                                    if (checkbox.checked) {
                                        modalContainer.style.setProperty('display', 'none', 'important');
                                        showBell = true;
                                    } else {
                                        modalContainer.style.setProperty('display', 'flex', 'important');
                                    }
                                } else {
                                    modalContainer.style.setProperty('display', 'flex', 'important');
                                }
                            });
        
                            updateLabel(showBell);
                        };
        
                        checkbox.addEventListener('change', checkModal);
                        setInterval(checkModal, 10);
                    }
                }
            ];
 
            const blockElements = () => {
                const style = document.createElement('style');
                style.id = 'block-elements-style';
                style.innerHTML = `
                    .item-generator_item-generator__TCQ9l,
                    .inventory_inventory__7bCIe,
                    .footer_footer__Mnt6p,
                    .missions_missions__haRAj,
                    .poll_poll__QyVsN {
                        display: none !important;
                    }
                `;
                document.head.appendChild(style);
            };
 
            const saveCheckboxState = (id, state) => {
                localStorage.setItem(id, JSON.stringify(state));
            };
 
            const loadCheckboxState = (id) => {
                return JSON.parse(localStorage.getItem(id)) || false;
            };
 
            const createMenu = () => {
                const menu = document.createElement('div');
                menu.className = 'luna-menu';
                menu.innerHTML = `
                    <div class=""luna-menu_title"">
                        <svg id=""toggle-icon"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                            <path fill-rule=""evenodd"" clip-rule=""evenodd"" d=""M19 8H5V10H7V12H9V14H11V16H13V14H15V12H17V10H19V8Z"" fill=""#f8ec94""></path>
                        </svg> <span class=""menu-title-text"">Clipping Tools</span>
                        <i id=""info-button"" class=""fa fa-info-circle"" aria-hidden=""true""></i>
                        </div>
                    <div id=""main-menu"">
                        ${menuOptions.map((option, index) => `
                            <div class=""luna-menu_item"" id=""menu-item-${index}"">
                                <input class=""luna-checkbox"" type=""checkbox"" id=""luna-checkbox-${index}"">
                                <label for=""luna-checkbox-${index}""> ${option.label}</label>
                            </div>
                        `).join('')}
                        </div>
                    </div>
                `;
                document.body.appendChild(menu);
 
                const menuTitle = menu.querySelector('.luna-menu_title');
                const toggleIcon = menu.querySelector('#toggle-icon');
                const infoButton = menu.querySelector('#info-button');
                menuTitle.addEventListener('click', () => {
                    menu.classList.toggle('collapsed');
                    const isCollapsed = menu.classList.contains('collapsed');
                    toggleIcon.style.transform = isCollapsed ? 'rotate(0deg)' : 'rotate(180deg)';
                    toggleIcon.style.setProperty('--drop-shadow', isCollapsed ? 'drop-shadow(2px 3px 0 #000000)' : 'drop-shadow(-2px -3px 0 #000000)');
                });
        
                infoButton.addEventListener('click', () => {
                    event.stopPropagation();
                    const modal = document.querySelector('.modal');
                    modal.classList.toggle('show');
                });
 
                toggleIcon.style.transform = 'rotate(180deg)';
                toggleIcon.style.setProperty('--drop-shadow', 'drop-shadow(2px 3px 0 #000000)');
 
 
                menuOptions.forEach((option, index) => {
                    const checkbox = menu.querySelector(`#luna-checkbox-${index}`);
                    const savedState = loadCheckboxState(`luna-checkbox-${index}`);
                    checkbox.checked = savedState;
                    if (savedState) {
                        option.action();
                    }
                    checkbox.addEventListener('change', function () {
                        saveCheckboxState(`luna-checkbox-${index}`, this.checked);
                        option.action();
                    });
                });
 
                const blockedWords = [""level"", ""gifted"", ""plushie""];
                const toastClass = ""toast_body__DVBLz"";
        
                function containsBlockedWords(text) {
                    return blockedWords.some(word => text.toLowerCase().includes(word));
                }
        
                function filterToasts() {
                    const toasts = document.querySelectorAll(`.${toastClass}`);
                    const filterCheckbox = document.querySelector('#luna-checkbox-6');
        
                    if (filterCheckbox && filterCheckbox.checked) {
                        toasts.forEach(toast => {
                            const toastText = toast.innerText || """";
                            console.log(`Processing toast message: ""${toastText.trim()}""`);
        
                            if (containsBlockedWords(toastText)) {
                                toast.style.display = ""none"";
                                console.log(`Blocked toast with content: ""${toastText.trim()}""`);
                            } else {
                                toast.style.display = """";
                                console.log(`Displayed toast with content: ""${toastText.trim()}""`);
                            }
                        });
                    }
                }
        
                setInterval(filterToasts, 10);
                return menu;
            };
 
            function insertMenuInSidebar() {
                const interval = setInterval(() => {
                    const sidebar = document.querySelector('.home_left__UiQ0z');
                    const targetDiv = sidebar ? sidebar.querySelector('.live-streams-monitoring-point_live-streams-monitoring-point__KOqPQ') : null;
 
                    if (sidebar && targetDiv) {
                        clearInterval(interval);
                        const menu = createMenu();
                        sidebar.insertBefore(menu, targetDiv);
                    }
                }, 1000);
            }
 
            const blockedDirectories = [
                ""https://cdn.fishtank.live/sounds/"",
                ""https://cdn.fishtank.live/sfx/""
            ];
 
            function interceptPlay() {
                const originalPlay = HTMLAudioElement.prototype.play;
                HTMLAudioElement.prototype.play = function() {
                    if (document.querySelector('#luna-checkbox-3').checked && blockedDirectories.some(dir => this.src.includes(dir))) {
                        return Promise.resolve();
                    }
                    return originalPlay.apply(this, arguments);
                };
            }
 
            function createResizeHandle() {
                const handle = document.createElement('div');
                handle.style.width = '100%';
                handle.style.height = '10px';
                handle.style.background = 'rgba(116, 0, 0, 0.5)';
                handle.style.cursor = 'ns-resize';
                handle.style.position = 'absolute';
                handle.style.left = '0';
                handle.style.bottom = '0';
                handle.style.zIndex = '1000';
                handle.classList.add('resize-handle');
                handle.addEventListener('mousedown', (e) => {
                    e.preventDefault();
                    const panelBody = handle.parentElement;
                    const list = panelBody.querySelector('.live-streams-monitoring-point_list__g0ojU');
                    const startY = e.clientY;
                    const startPanelHeight = panelBody.offsetHeight;
 
                    const onMouseMove = (e) => {
                        const newHeight = startPanelHeight + (e.clientY - startY);
                        const minHeight = 100;
                        const clampedHeight = Math.max(minHeight, newHeight);
                        panelBody.style.height = clampedHeight + 'px';
                        if (list) {
                            list.style.height = clampedHeight + 'px';
                        }
                    };
 
                    const onMouseUp = () => {
                        document.removeEventListener('mousemove', onMouseMove);
                        document.removeEventListener('mouseup', onMouseUp);
                    };
 
                    document.addEventListener('mousemove', onMouseMove);
                    document.addEventListener('mouseup', onMouseUp);
                });
 
                return handle;
            }
 
            function addResizeHandle() {
                const panelBody = document.querySelector('.panel_panel__Tdjid.panel_full-height__2dCSF.panel_no-padding__woODX');
                if (panelBody) {
                    const list = panelBody.querySelector('.live-streams-monitoring-point_list__g0ojU');
                    if (list) {
                        list.style.maxHeight = 'none';
                    }
                    panelBody.style.position = 'relative';
                    panelBody.style.overflow = 'hidden';
                    if (panelBody.offsetHeight > 0) {
                        const existingHandle = panelBody.querySelector('.resize-handle');
                        if (!existingHandle) {
                            const handle = createResizeHandle();
                            handle.classList.add('resize-handle');
                            panelBody.appendChild(handle);
                        }
                    }
                }
            }
 
            function waitForPanel() {
                const checkInterval = setInterval(() => {
                    const panelBody = document.querySelector('.panel_panel__Tdjid.panel_full-height__2dCSF.panel_no-padding__woODX');
                    if (panelBody) {
                        addResizeHandle();
                        clearInterval(checkInterval);
                    }
                }, 1000);
            }
 
            window.addEventListener('load', () => {
                waitForPanel();
            });
 
            const panelObserver = new MutationObserver(() => {
                const existingHandle = document.querySelector('.panel_panel__Tdjid.panel_full-height__2dCSF.panel_no-padding__woODX .resize-handle');
                if (!existingHandle) {
                    waitForPanel();
                }
            });
 
            const keyBindings = {
                screenshot: 'Alt+S',
                fullscreen: 'Alt+F',
                pip1: 'Alt+P'
            };
    
            const savedKeyBindings = localStorage.getItem('keyBindings');
            if (savedKeyBindings) {
                Object.assign(keyBindings, JSON.parse(savedKeyBindings));
            }
    
            const createModal = () => {
                const modal = document.createElement('div');
                modal.className = 'modal';
                modal.innerHTML = `
                    <div class=""modal-title""><h2 style=""padding-top:5px; padding-bottom:5px;"">Clipping Tools++ Info</h2></div>
                    <ul style=""padding-left:10px;max-width:400px;"">
                        ${menuOptions.map(option => `
                            <li style=""padding:10px;list-style-type: none;""><strong>${option.label}:</strong><p style=""font-size:10px;""> ${option.description}</p></li>
                        `).join('')}
                        <li style=""padding:10px;list-style-type: none;"">
                            <strong>Screenshot:</strong>
                            <button class=""change-key"" id=""change-screenshot-key"">Click Here To Change (${keyBindings.screenshot})</button>
                        </li>
                        <li style=""padding:10px;list-style-type: none;"">
                            <strong>Fullscreen:</strong>
                            <button class=""change-key"" id=""change-fullscreen-key"">Click Here To Change (${keyBindings.fullscreen})</button>
                        </li>
                        <li style=""padding:10px;list-style-type: none;"">
                            <strong>PiP:</strong>
                            <button style=""margin-left:66px;"" class=""change-key"" id=""change-pip1-key"">Click Here To Change (${keyBindings.pip1})</button>
                        </li>
                    </ul>
                    <button id=""close-modal"" style=""margin-bottom:10px;"">Close</button>
                    <div class=""button-container"">
                        <a href=""https://x.com/luna__mae"" target=""_blank"" id=""twitter-link"">
                            <button>designed with love by @luna__mae <i style=""font-size:12px;"" class=""fa-brands fa-square-x-twitter""></i></button>
                        </a>         
                    </div>       
                `;
                document.body.appendChild(modal);
                
                const closeModalButton = modal.querySelector('#close-modal');
                closeModalButton.addEventListener('click', () => {
                    modal.classList.remove('show');
                });
    
                setTimeout(() => {
                    document.querySelector('#change-screenshot-key').addEventListener('click', () => changeKey('screenshot'));
                    document.querySelector('#change-fullscreen-key').addEventListener('click', () => changeKey('fullscreen'));
                    document.querySelector('#change-pip1-key').addEventListener('click', () => changeKey('pip1'));
                }, 0);
            };
    
            const changeKey = (action) => {
                const buttonSelector = `#change-${action}-key`;
                const button = document.querySelector(buttonSelector);
                if (!button) {
                    console.error(`Button not found for action: ${action}`);
                    return;
                }
                button.textContent = 'Waiting...';
        
                const keyCombination = new Set();
        
                const keyDownListener = (event) => {
    
                    if (['Control', 'Alt', 'Shift'].includes(event.key)) {
                        keyCombination.add(event.key);
                    } else {
                        const combo = [];
                        if (keyCombination.has('Control')) combo.push('Ctrl');
                        if (keyCombination.has('Alt')) combo.push('Alt');
                        if (keyCombination.has('Shift')) combo.push('Shift');
                        combo.push(event.key.toUpperCase());
    
                        keyBindings[action] = combo.join('+');
                        button.textContent = `Click Here To Change (${keyBindings[action]})`;
    
                        localStorage.setItem('keyBindings', JSON.stringify(keyBindings));
    
                        keyCombination.clear();
                        document.removeEventListener('keydown', keyDownListener);
                        document.removeEventListener('keyup', keyUpListener);
                    }
                };
    
                const keyUpListener = (event) => {
                    if (['Control', 'Alt', 'Shift'].includes(event.key)) {
                        keyCombination.delete(event.key);
                    }
                };
    
                document.addEventListener('keydown', keyDownListener);
                document.addEventListener('keyup', keyUpListener);
            };
    
            createModal();
    
 
            const scriptId = '514950';
            const updateCheckInterval = 1 * 60 * 60 * 1000;
            const currentVersion = '3.3.5';
            const initialDelay = 2000;
    
            const checkForUpdates = async () => {
                try {
                    const proxyUrl = 'https://corsproxy.io/?';
                    const targetUrl = `https://update.greasyfork.org/scripts/${scriptId}/Clipping%20Tools%2B%2Bmeta.js`;
                    const response = await fetch(proxyUrl + targetUrl);
                    const meta = await response.text();
                    const versionMatch = meta.match(/@version\s+(\d+\.\d+\.\d+)/);
                    if (versionMatch) {
                        const latestVersion = versionMatch[1];
                        if (latestVersion !== currentVersion) {
                            displayUpdateToast(latestVersion);
                        }
                    }
                } catch (error) {
                    console.error('Failed to check for updates:', error);
                }
            };
 
            function captureScreenshot() {
                const videoElement = document.querySelector('video');
                if (videoElement) {
                    const canvas = document.createElement('canvas');
                    canvas.width = videoElement.videoWidth;
                    canvas.height = videoElement.videoHeight;
                    const context = canvas.getContext('2d');
                    context.drawImage(videoElement, 0, 0, canvas.width, canvas.height);
                    const dataURL = canvas.toDataURL('image/png');
    
                    const link = document.createElement('a');
                    link.href = dataURL;
                    link.download = 'FTS3SC.png';
                    link.click();
                }
            }
    
            let activeKeys = new Set();
 
            document.addEventListener('keydown', (event) => {
                activeKeys.add(event.key);
    
                const keyCombination = [];
                if (activeKeys.has('Control')) keyCombination.push('Ctrl');
                if (activeKeys.has('Alt')) keyCombination.push('Alt');
                if (activeKeys.has('Shift')) keyCombination.push('Shift');
        
                if (!['Control', 'Alt', 'Shift'].includes(event.key)) {
                    keyCombination.push(event.key.toUpperCase());
                }
    
                const pressedKey = keyCombination.join('+');
        
                if (pressedKey === keyBindings.screenshot) {
                    captureScreenshot();
                }
                if (pressedKey === keyBindings.fullscreen) {
                    toggleFullscreen();
                }
                if (pressedKey === keyBindings.pip1) {
                    togglePiPMode();
                }
            });
    
            document.addEventListener('keyup', (event) => {
                activeKeys.delete(event.key);
            });
    
            function togglePiPMode() {
                const videoElement = document.querySelector('video');
                if (videoElement) {
                    if (document.pictureInPictureElement) {
                        document.exitPictureInPicture();
                    } else {
                        videoElement.requestPictureInPicture();
                    }
                }
            }
      
    
            function toggleFullscreen() {
                const videoElement = document.querySelector('video');
                if (videoElement) {
                    if (!document.fullscreenElement) {
                        if (videoElement.requestFullscreen) {
                            videoElement.requestFullscreen();
                        } else if (videoElement.mozRequestFullScreen) { 
                            videoElement.mozRequestFullScreen();
                        } else if (videoElement.webkitRequestFullscreen) { 
                            videoElement.webkitRequestFullscreen();
                        } else if (videoElement.msRequestFullscreen) { 
                            videoElement.msRequestFullscreen();
                        }
                    } else {
                        if (document.exitFullscreen) {
                            document.exitFullscreen();
                        } else if (document.mozCancelFullScreen) { 
                            document.mozCancelFullScreen();
                        } else if (document.webkitExitFullscreen) { 
                            document.webkitExitFullscreen();
                        } else if (document.msExitFullscreen) { 
                            document.msExitFullscreen();
                        }
                    }
                }
            }
    
            function togglePictureInPicture() {
                const videoElement = document.querySelector('video');
                const pipCheckbox = document.querySelector('#luna-checkbox-1');
                if (videoElement && pipCheckbox) {
                    if (pipCheckbox.checked) {
                        document.exitPictureInPicture();
                        pipCheckbox.checked = false;
                    } else {
                        videoElement.requestPictureInPicture();
                        pipCheckbox.checked = true;
                    }
                }
            }
    
            document.addEventListener('leavepictureinpicture', () => {
                const pipCheckbox = document.querySelector('#luna-checkbox-1');
                if (pipCheckbox) {
                    pipCheckbox.checked = false;
                }
            });
            const displayUpdateToast = (latestVersion) => {
                const toast = document.createElement('div');
                toast.className = 'custom-toast';
                toast.innerHTML = `
                    <div class=""custom-toast-message"">
                        <div class=""custom-toast-icon"">
                            <svg fill=""none"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" width=""32"" height=""32"">
                                <path fill-rule=""evenodd"" clip-rule=""evenodd"" d=""M15 6H17V8H15V6ZM13 10V8H15V10H13ZM11 12V10H13V12H11ZM9 14V12H11V14H9ZM7 16V14H9V16H7ZM5 16H7V18H5V16ZM3 14H5V16H3V14ZM3 14H1V12H3V14ZM11 16H13V18H11V16ZM15 14V16H13V14H15ZM17 12V14H15V12H17ZM19 10V12H17V10H19ZM21 8H19V10H21V8ZM21 8H23V6H21V8Z"" fill=""#f8ec94""></path>
                            </svg>                    
                        </div>
                        <p>New Clipping Tools++ version ${latestVersion} available! <a href=""https://greasyfork.org/scripts/${scriptId}"" target=""_blank"">Update now</a></p>
                        <div class=""custom-toast-close"">
                            <button class=""custom-close-button"" type=""button"">
                            <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                                <path d=""M5 3H3v18h18V3H5zm14 2v14H5V5h14zm-8 4H9V7H7v2h2v2h2v2H9v2H7v2h2v-2h2v-2h2v2h2v2h2v-2h-2v-2h-2v-2h2V9h2V7h-2v2h-2v2h-2V9z"" fill=""currentColor""></path>
                            </svg>
                            </button>
                        </div>
                    </div>
                `;
    
                const mainElement = document.querySelector('main');
                const tooltipElement = document.getElementById('tooltip');
    
                if (mainElement && tooltipElement) {
                    mainElement.insertAdjacentElement('afterend', toast);
                } else {
                    console.error('Failed to find the main element or tooltip element.');
                }
    
                const closeButton = toast.querySelector('.custom-close-button');
                closeButton.addEventListener('click', () => {
                    toast.remove();
                });
            };
    
            setTimeout(checkForUpdates, initialDelay);
    
            setInterval(checkForUpdates, updateCheckInterval);
    
            panelObserver.observe(document.body, { childList: true, subtree: true });
            createModal();
            insertMenuInSidebar();   
        })();




        setTimeout(() => {
            (function() {
                'use strict';
 
                const style = document.createElement('style');
                style.innerHTML = `
                    .livepeer-video-player_livepeer-video-player__NRXYi .livepeer-video-player_controls__y36El .livepeer-video-player_volume-controls__q9My4,
                    .livepeer-video-player_livepeer-video-player__NRXYi .livepeer-video-player_controls__y36El .livepeer-video-player_clipping__GlB4S {
                        visibility: hidden;
                        display: flex;
                        align-items: center;
                        width: 100%;
                        background-color: #191d21;
                        font-size: 14px;
                        border-radius: 4px;
                        border: 1px solid #505050;
                    }
 
                    .luna-menu {
                        top: 10px;
                        left: 10px;
                        background: rgba(25, 29, 33, 1);
                        color: white;
                        padding: 0px;
                        font-size: 14px;
                        border-radius: 4px;
                        z-index: 5;
                        border: 1px solid #505050;
                        cursor: default;
                    }
                    .luna-menu.collapsed #main-menu {
                        display: none;
                    }
                    .luna-menu.collapsed .luna-menu_title {
                        border-bottom: none;
                    }
                    .luna-menu_title {
                        font-weight: bold;
                        padding: 4px 8px;
                        cursor: pointer;
                        margin-bottom: 0px;
                        background: rgba(116, 7, 0, 1);
                        border-top-left-radius: 4px;
                        border-top-right-radius: 4px;
                        border-bottom: 1px solid #505050;
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                    }
                    .luna-menu_title:hover {
                        background-color: #a70a00;
                    }
                    .luna-menu_item {
                        margin: 5px 0;
                        padding: 3px;
                        cursor: pointer;
                        display: flex;
                        align-items: center;
                    }
                    .luna-menu_item:hover {
                        background-color: hsla(0, 0%, 100%, .1);
                        color: #f8ec94;
                    }
                    .luna-hide-scan_lines::after {
                        content: none !important;
                    }
                    .luna-checkbox {
                        appearance: none;
                        margin-right: 5px;
                        width: 20px;
                        height: 20px;
                        background-color: #303438;
                        border: 2px solid black;
                        border-radius: 3px;
                        position: relative;
                    }
                    .luna-checkbox:checked {
                        background-color: rgba(116, 7, 0, 1);
                    }
                    .luna-checkbox:checked::after {
                        content: '';
                        position: absolute;
                        left: 3px;
                        top: 3px;
                        width: 10px;
                        height: 10px;
                        background-color: #f8ec94;
                    }
                    .luna-checkbox input:checked + .luna-checkbox::after {
                        display: block;
                    }
                    #toggle-icon {
                        transition: transform 0.5s, filter 0.9s;
                        --drop-shadow: drop-shadow(2px 3px 0 #000000);
                        filter: var(--drop-shadow);
                    }
                    .luna-menu_title svg {
                        margin-left: -3px;
                        color: #f8ec94;
                    }
                    .menu-title-text {
                        flex-grow: 1;
                        margin-left: 4px;
                        padding-left: 0px;
                    }
                    #toggle-icon {
                        transition: transform 0.5s, filter 0.9s;
                        --drop-shadow: drop-shadow(2px 3px 0 #000000);
                        filter: var(--drop-shadow);
                    }
                    #custom-volume-slider input[type=""range""] {
                        -webkit-appearance: none;
                        appearance: none;
                        width: 100%;
                        height: 8px;
                        background: linear-gradient(to right, #740700 0%, #740700 var(--value), #555 var(--value), #555 100%);
                        border-radius: 5px;
                        outline: none;
                    }
 
                    #custom-volume-slider input[type=""range""]::-webkit-slider-thumb {
                        -webkit-appearance: none;
                        appearance: none;
                        width: 16px;
                        height: 16px;
                        border-radius: 50%;
                        border: 1px solid #505050;
                        background: #740700;
                        cursor: pointer;
                        box-shadow: 0 0 2px rgba(0, 0, 0, 0.5);
                    }
 
                    #custom-volume-slider input[type=""range""]::-moz-range-thumb {
                        width: 16px;
                        height: 16px;
                        border-radius: 50%;
                        border: 1px solid #505050;
                        background: #740700;
                        cursor: pointer;
                        box-shadow: 0 0 2px rgba(0, 0, 0, 0.5);
                    }
 
                    #custom-volume-slider input[type=""range""]:hover {
                        background: linear-gradient(to right, #740700 0%, #740700 var(--value), #555 var(--value), #555 100%);
                    }
                    .custom-toast {
                        position: fixed;
                        bottom: 20px;
                        right: 20px;
                        background-color: rgba(25, 29, 33, 1);
                        color: #fff;
                        padding: 15px;
                        border-radius: 5px;
                        border: 4px solid #f8ec94;            
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        z-index: 10000;
                        display: flex;
                        align-items: center;
                        gap: 10px;
                    }
                    .custom-toast-message {
                        display: flex;
                        align-items: center;
                        gap: 10px;
                    }
                    .custom-toast-icon svg {
                        fill: #fff;
                    }
                    .custom-toast-close {
                        margin-left: auto;
                    }          
                    .custom-close-button {
                        background: none;
                        border: none;
                        cursor: pointer;
                    }
                    .custom-close-button svg {
                        fill: #fff;
                    }                             
                `;
                document.head.appendChild(style);
 
                let blockQualityCheckbox;
   
                const initialDelay = 1000;
                const updateCheckInterval = 300000;
                const currentVersion = '2.7.1';
 
                const checkForUpdates = async () => {
                    try {
                        const proxyUrl = 'https://corsproxy.io/?';
                        const targetUrl = 'https://update.greasyfork.org/scripts/515300/Clipping%20Tools%2B%2Bmeta.js';
        
                        const response = await fetch(proxyUrl + targetUrl);
        
                        if (!response.ok) {
                            throw new Error(`Network response was not ok, status ${response.status}`);
                        }
        
                        const meta = await response.text();
                        const versionMatch = meta.match(/@version\s+(\d+\.\d+\.\d+)/);
                        if (versionMatch) {
                            const latestVersion = versionMatch[1];
        
                            if (latestVersion !== currentVersion) {
                                displayUpdateToast(latestVersion);
                            }
                        }
                    } catch (error) {
                        console.error('Update check error:', error);
                    }
                };
        
                const displayUpdateToast = (latestVersion) => {
                    const toast = document.createElement('div');
                    toast.className = 'custom-toast';
                    toast.innerHTML = `
                        <div class=""custom-toast-message"">
                        <div class=""custom-toast-icon"">
                            <svg fill=""none"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" width=""32"" height=""32"">
                                <path fill-rule=""evenodd"" clip-rule=""evenodd"" d=""M15 6H17V8H15V6ZM13 10V8H15V10H13ZM11 12V10H13V12H11ZM9 14V12H11V14H9ZM7 16V14H9V16H7ZM5 16H7V18H5V16ZM3 14H5V16H3V14ZM3 14H1V12H3V14ZM11 16H13V18H11V16ZM15 14V16H13V14H15ZM17 12V14H15V12H17ZM19 10V12H17V10H19ZM21 8H19V10H21V8ZM21 8H23V6H21V8Z"" fill=""#f8ec94""></path>
                            </svg>                    
                        </div>
                            <span>New Livestream Controls version available: ${latestVersion} <a href=""https://greasyfork.org/scripts/515300"" target=""_blank"">Update now</a></span>
                            <div class=""custom-toast-close"">
                                <button class=""custom-close-button"" type=""button"">
                                <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                                    <path d=""M5 3H3v18h18V3H5zm14 2v14H5V5h14zm-8 4H9V7H7v2h2v2h2v2H9v2H7v2h2v-2h2v-2h2v2h2v2h2v-2h-2v-2h-2v-2h2V9h2V7h-2v2h-2v2h-2V9z"" fill=""currentColor""></path>
                                </svg>
                                </button>
                            </div>
                        </div>
                    `;
        
                    document.body.appendChild(toast);
        
                    const closeButton = toast.querySelector('.custom-close-button');
                    closeButton.addEventListener('click', () => {
                        toast.remove();
                    });
                };
        
 
                setTimeout(() => {
                    checkForUpdates();
                }, initialDelay);
        
                setInterval(() => {
                    checkForUpdates();
                }, updateCheckInterval);
 
                function createVolumeSliderWithRecording() {
                    const container = document.createElement('div');
                    container.id = 'custom-volume-slider';
                    container.classList.add('luna-menu');
                    container.style.color = '#fff';
 
                    const header = document.createElement('div');
                    header.classList.add('luna-menu_title');
                    header.style.marginBottom = '0px';
 
                    header.innerHTML = `
                        <svg id=""toggle-icon"" width=""14"" height=""14"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                            <path fill-rule=""evenodd"" clip-rule=""evenodd"" d=""M19 8H5V10H7V12H9V14H11V16H13V14H15V12H17V10H19V8Z"" fill=""#f8ec94""></path>
                        </svg><span class=""menu-title-text"">Livestream Controls</span>
                    `;
 
                    container.appendChild(header);
 
                    const content = document.createElement('div');
                    content.style.display = 'none';
 
                    const sliderLabel = document.createElement('span');
                    sliderLabel.innerText = 'Volume:';
                    sliderLabel.style.marginRight = '10px';
                    sliderLabel.style.marginTop = '5px';
                    sliderLabel.style.marginLeft = '5px';
 
                    const volumeSlider = document.createElement('input');
                    volumeSlider.type = 'range';
                    volumeSlider.min = '0.00001';
                    volumeSlider.max = '1';
                    volumeSlider.step = '0.01';
                    volumeSlider.value = '1';
                    volumeSlider.style.background = '#740700';
                    volumeSlider.style.border = '1px solid #505050';
                    volumeSlider.style.flex = '1';
                    volumeSlider.style.marginRight = '5px';
                    volumeSlider.classList.add('custom-volume-slider');
 
                    volumeSlider.addEventListener('input', function() {
                        const videoElement = document.querySelector('video[data-livepeer-video]');
                        if (videoElement) {
                            videoElement.volume = this.value;
                            videoElement.setAttribute('data-livepeer-volume', Math.round(this.value * 100));
                            console.log(`Volume set to ${this.value}`);
                        }
                        this.style.background = `linear-gradient(to right, #740700 ${this.value * 100}%, #555 ${this.value * 100}%)`;
                    });
 
                    volumeSlider.style.background = `linear-gradient(to right, #740700 ${volumeSlider.value * 100}%, #555 ${volumeSlider.value * 100}%)`;
 
                    const volumeContainer = document.createElement('div');
                    volumeContainer.style.display = 'flex';
                    volumeContainer.style.alignItems = 'center';
                    volumeContainer.style.width = '100%';
 
                    volumeContainer.appendChild(sliderLabel);
                    volumeContainer.appendChild(volumeSlider);
 
                    const recordingContainer = document.createElement('div');
                    recordingContainer.style.display = 'flex';
                    recordingContainer.style.justifyContent = 'space-between';
                    recordingContainer.style.width = '100%';
                    recordingContainer.style.marginTop = '10px';
 
                    const startButton = document.createElement('button');
                    startButton.innerText = 'Start Recording';
                    startButton.style.marginRight = '0px';
                    startButton.style.padding = '10px';
                    startButton.style.flex = '1';
                    startButton.style.fontSize= '10px';
                    startButton.style.backgroundColor = '#303438';
                    startButton.style.border = '2px solid black';
                    startButton.style.borderRight = '1px solid black';
                    startButton.style.color = '#fff';
                    startButton.style.cursor = 'pointer';
 
                    startButton.addEventListener('mouseover', function() {
                        startButton.style.color = '#f8ec94';
                    });
 
                    startButton.addEventListener('mouseout', function() {
                        startButton.style.color = '#fff';
                    });
 
                    const lastMinuteButton = document.createElement('button');
                    lastMinuteButton.innerText = 'Record Last Minute';
                    lastMinuteButton.style.padding = '0px';
                    lastMinuteButton.style.flex = '1';
                    lastMinuteButton.style.fontSize= '10px';            
                    lastMinuteButton.style.backgroundColor = '#303438';
                    lastMinuteButton.style.border = '2px solid black';
                    lastMinuteButton.style.borderLeft = '1px solid black';
                    lastMinuteButton.style.color = '#fff';
                    lastMinuteButton.style.cursor = 'pointer';
 
                    lastMinuteButton.addEventListener('mouseover', function() {
                        lastMinuteButton.style.color = '#f8ec94';
                    });
 
                    lastMinuteButton.addEventListener('mouseout', function() {
                        lastMinuteButton.style.color = '#fff';
                    });
 
                    content.appendChild(volumeContainer);
                    recordingContainer.appendChild(startButton);
                    recordingContainer.appendChild(lastMinuteButton);
                    content.appendChild(recordingContainer);
 
                    const menuItem = document.createElement('div');
                    menuItem.classList.add('luna-menu_item');
 
                    blockQualityCheckbox = document.createElement('input');
                    blockQualityCheckbox.type = 'checkbox';
                    blockQualityCheckbox.id = 'block-quality-checkbox';
                    blockQualityCheckbox.classList.add('luna-checkbox');
                    blockQualityCheckbox.style.marginTop = '5px';
                    blockQualityCheckbox.style.marginLeft = '5px';
 
                    const blockQualityLabel = document.createElement('label');
                    blockQualityLabel.htmlFor = 'block-quality-checkbox';
                    blockQualityLabel.innerText = 'Block Quality';
                    blockQualityLabel.style.marginLeft = '3px';
 
                    menuItem.appendChild(blockQualityCheckbox);
                    menuItem.appendChild(blockQualityLabel);
 
                    content.appendChild(menuItem);
 
                    const isChecked = localStorage.getItem('blockQualityCheckbox') === 'true';
                    blockQualityCheckbox.checked = isChecked;
 
                    blockQualityCheckbox.addEventListener('change', () => {
                        localStorage.setItem('blockQualityCheckbox', blockQualityCheckbox.checked);
                    });                  
 
                    volumeSlider.addEventListener('input', function() {
                        const videoElement = document.querySelector('video[data-livepeer-video]');
                        if (videoElement) {
                            videoElement.volume = this.value;
                            videoElement.setAttribute('data-livepeer-volume', Math.round(this.value * 100));
                            console.log(`Volume set to ${this.value}`);
                        }
                    });
 
                    let isRecording = false;
                    startButton.addEventListener('click', function() {
                        const event = new KeyboardEvent('keydown', {
                            key: 'c',
                            code: 'KeyC',
                            ctrlKey: false,
                            shiftKey: false,
                            altKey: false,
                            metaKey: false,
                            bubbles: true,
                            cancelable: true
                        });
                        document.dispatchEvent(event);
 
                        isRecording = !isRecording;
                        startButton.innerText = isRecording ? 'Stop Recording' : 'Start Recording';
                        if (isRecording) {
                            startButton.style.backgroundColor = '#740700';
                            startButton.style.color = '#f8ec94';
                        } else {
                            startButton.style.backgroundColor = '#555';
                            startButton.style.color = '#fff';
                        }
                    });
 
                    lastMinuteButton.addEventListener('click', function() {
                        const event = new KeyboardEvent('keydown', {
                            key: 'c',
                            code: 'KeyC',
                            ctrlKey: false,
                            shiftKey: true,
                            altKey: false,
                            metaKey: false,
                            bubbles: true,
                            cancelable: true
                        });
                        document.dispatchEvent(event);
                    });
 
                    container.appendChild(content);
 
                    const toggleIcon = header.querySelector('#toggle-icon');
 
                    header.addEventListener('click', function() {
                        if (content.style.display === 'none') {
                            content.style.display = 'block';
                        } else {
                            content.style.display = 'none';
                        }
 
                        const isCollapsed = content.style.display === 'none';
                        toggleIcon.style.transform = isCollapsed ? 'rotate(0deg)' : 'rotate(180deg)';
                        toggleIcon.style.setProperty('--drop-shadow', isCollapsed ? 'drop-shadow(2px 3px 0 #000000)' : 'drop-shadow(-2px -3px 0 #000000)');
                    });
 
 
                    return container;
                }
 
                function moveControls() {
                    const sidebar = document.querySelector('.home_left__UiQ0z');
                    const adsDiv = document.querySelector('.live-streams-monitoring-point_live-streams-monitoring-point__KOqPQ');
 
                    if (!sidebar || !adsDiv) {
                        console.log(""Sidebar or Ads div not found. Retrying..."");
                        return;
                    }
 
                    let customSlider = document.getElementById('custom-volume-slider');
                    if (!customSlider) {
                        customSlider = createVolumeSliderWithRecording();
                        sidebar.insertBefore(customSlider, adsDiv);
                    }
 
                    const volumeControls = document.querySelector('.livepeer-video-player_volume-controls__q9My4');
                    if (volumeControls && !volumeControls.classList.contains('moved')) {
                        console.log(""Moving volume controls..."");
                        volumeControls.classList.add('moved');
                        volumeControls.style.display = 'none';
                    }
 
                    const clippingControls = document.querySelector('.livepeer-video-player_clipping__GlB4S');
                    if (clippingControls && !clippingControls.classList.contains('moved')) {
                        console.log(""Moving clipping controls..."");
                        clippingControls.classList.add('moved');
                        clippingControls.style.display = 'none';
                    }
 
                    hideQualityButton();
                }
 
                function hideQualityButton() {
                    const qualityDiv = document.querySelector('.livepeer-video-player_quality__1WPkz');
                    if (blockQualityCheckbox.checked && qualityDiv) {
                        qualityDiv.style.display = 'none';
                        console.log(""Quality button hidden."");
                    } else if (qualityDiv) {
                        qualityDiv.style.display = 'block';
                    }
                }
 
                const blockQualityState = JSON.parse(localStorage.getItem('block-quality-checkbox')) || false;
 
                setInterval(moveControls, 100);
 
                blockQualityCheckbox.checked = blockQualityState;
 
                blockQualityCheckbox.addEventListener('change', function() {
                    const isChecked = this.checked;
                    localStorage.setItem('block-quality-checkbox', JSON.stringify(isChecked));
                    hideQualityButton();
                });
 
                setInterval(hideQualityButton, 100);
 
            })();
        }, 1000);


        (function() {
            'use strict';
 
            const styleLinks = [
                'https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.css',
                'https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.8.1/nouislider.min.css'
            ];
    
            styleLinks.forEach(link => {
                const linkElement = document.createElement('link');
                linkElement.rel = 'stylesheet';
                linkElement.href = link;
                document.head.appendChild(linkElement);
            });
 
            const style = document.createElement('style');
            style.type = 'text/css';

            // JavaScript links to be added
            const scriptLinks = [
                'https://cdnjs.cloudflare.com/ajax/libs/gifshot/0.3.2/gifshot.min.js',
                'https://cdnjs.cloudflare.com/ajax/libs/cropperjs/1.6.2/cropper.min.js',
                'https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js',
                'https://cdnjs.cloudflare.com/ajax/libs/noUiSlider/15.8.1/nouislider.min.js'
            ];

            // Function to add the scripts to the document
            scriptLinks.forEach(src => {
                const scriptElement = document.createElement('script');
                scriptElement.src = src;
                scriptElement.type = 'text/javascript';
                document.head.appendChild(scriptElement);
            });
 
            style.innerHTML = `
                .noUi-connect {
                    background: #740700 !important;
                }
                .noUi-target {
                    background: rgba(25, 29, 33, 1) !important;     
                }           
                .giffy {
                    display: inline-block;
                    width: 100px;
                    font-size: 10px;
                    padding: 5px 10px;
                    background-color: rgba(25, 29, 33, 1);
                    border: 1px solid #505050;
                    border-radius: 0;
                    cursor: pointer;
                    transition: color 0.3s, outline 0.3s;
                    box-sizing: border-box;
                }
 
                .giffy:hover {
                    outline: 2px solid #f8ec94;
                    color: #f8ec94;
                } 
                .cropper-view-box {
                    outline: 1px solid #ff0000 !important;
                    outline-color: rgb(255 0 0 / 75%) !important;
                }
                .cropper-face {
                    background-color: rgb(255 118 118 / 60%) !important;
                }
                .cropper-line {
                    background-color: #ff0000 !important;
                }            
                .cropper-point {
                    background-color: rgb(255 0 0 / 50%) !important;
                }
                .centerx {
                    display: flex;
                    justify-content: center;
                    align-items: center;  
                }       
                .noUi-handle {
                    background: #505050 !important;
                    box-shadow: inset 0 0 1px #FFF, inset 0 1px 7px #898989, 0 3px 6px -3px #BBB !important;
                    border: 1px solid #505050 !important;
                }
            `;
 
            document.head.appendChild(style);    
 
            const FRAME_INTERVAL = 100;
            const MAX_DURATION = 10000; 
            const frames = [];
            let recording = true;
            let cropper;
 
            const gifButton = document.createElement('button');
            gifButton.innerHTML = '<img src=""https://raw.githubusercontent.com/luna-mae/ClippingTools/refs/heads/main/media/GIF.png"" alt=""GIF"" style=""height: 26px; vertical-align: middle; margin-right:-5px;""> <span style=""position: relative; top: 2px;"">CLIPPER</span>';
            gifButton.style.backgroundColor = '#740700';
            gifButton.style.color = 'white';
            gifButton.style.padding = '2px';
            gifButton.style.border = '1px solid rgb(255 0 0 / 25%)';
            gifButton.style.borderRadius = '4px';
            gifButton.style.cursor = 'pointer';
            gifButton.style.marginBottom = '3px';
 
            gifButton.addEventListener('mouseover', () => {
                gifButton.style.backgroundColor = '#8c0b00';
                gifButton.style.border = '1px solid rgb(255 0 0)';        
            });
 
            gifButton.addEventListener('mouseout', () => {
                gifButton.style.backgroundColor = '#740700';
                gifButton.style.border = '1px solid rgb(255 0 0 / 25%)';    
            });
    
            const checkForMonitoringPoint = () => {
                const monitoringPointDiv = document.querySelector('.live-streams-monitoring-point_live-streams-monitoring-point__KOqPQ');
                if (monitoringPointDiv) {
                    monitoringPointDiv.parentNode.insertBefore(gifButton, monitoringPointDiv);
                    observer.disconnect();
                }
            };
 
            const observer = new MutationObserver((mutations) => {
                mutations.forEach(() => {
                    checkForMonitoringPoint();
                });
            });
 
            observer.observe(document.body, { childList: true, subtree: true });
 
            checkForMonitoringPoint();
    
            gifButton.addEventListener('click', () => {
                recording = false;
                openEditor();
            });
    
 
            const captureFrames = () => {
                if (recording) {
                    const video = document.querySelector('video');
                    if (video && video.readyState >= 2) { 
                        const canvas = document.createElement('canvas');
                        canvas.width = video.videoWidth;
                        canvas.height = video.videoHeight;
                        const ctx = canvas.getContext('2d', { willReadFrequently: true });
                        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                        frames.push(canvas.toDataURL('image/webp'));
    
                        const maxFrames = MAX_DURATION / FRAME_INTERVAL;
                        if (frames.length > maxFrames) frames.shift();
                    }
                }
            };
 
            setInterval(captureFrames, FRAME_INTERVAL);
 
            function openEditor() {
                const editor = document.createElement('div');
                editor.style.position = 'fixed';
                editor.style.top = '50%';
                editor.style.left = '50%';
                editor.style.transform = 'translate(-50%, -50%)';
                editor.style.backgroundColor = ' rgba(25, 29, 33, 1)';
                editor.style.outline = '2px solid #505050';
                editor.style.paddingBottom = '10px';
                editor.style.zIndex = 1001;
                editor.style.borderRadius = '10px';
                editor.style.boxShadow = '0 4px 8px rgba(0, 0, 0, 0.2)';
                editor.innerHTML = `
                    <div style=""background-color: #740700; border-top-left-radius:5px; border-top-right-radius:5px; padding-top:10px; margin-bottom:10px;"">
                    <h2 style=""padding-bottom:5px; color:#fff; margin-left:5px;border-bottom: 1px solid #505050;"">Edit GIF</h2>
                    </div>
                    <div id=""previewContainer"" style=""width: 100%; overflow: hidden; display: flex; flex-direction: column; align-items: center; padding-left:150px; padding-right: 150px;"">
                        <div class=""centerx"">
                            <h3 style=""color:#fff; margin-bottom:10px;margin-top:10px;"">Crop GIF:</h3>
                        </div>
                    <div style=""position: relative;"">
                        <img id=""staticFrameImage"" style=""max-width: 100%; max-height: 300px; border: 1px solid #ddd;"">
                        <p id=""loadingMessage"" style=""position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);"">Loading GIF...</p>
                    </div>
                    <div class=""centerx"">
                        <h3 style=""color:#fff; margin-bottom:10px;margin-top:10px;"">Trim GIF:</h3>
                    </div>            
                    <img id=""previewImage"" style=""max-width: 100%; max-height: 300px; border: 1px solid #ddd; margin-top: 10px; margin-bottom:15px; display: none;"">
                    </div>
                    <label style=""margin-left:25px;margin-bottom: 15px; padding-top:15px;"">Trim GIF Length:</label>
                    <div class=""centerx"">
                    <div id=""gifLengthSlider"" style=""width: 90%; margin-top: 10px;""></div>
                    </div>
                    <div style=""padding-top:15px;"">
                    <span id=""lengthDisplay"" style=""margin-top: 5px;margin-left:25px;""></span>
                    <div class=""centerx"" style=""margin-top:10px;"">
                        <label for=""qualityDropdown"" style=""color:#fff; margin-right:10px;"">Export Quality:</label>
                        <select id=""qualityDropdown"" class=""giffy"">
                            <option value=""10"">10</option>
                            <option value=""9"">9</option>
                            <option value=""8"" selected>8</option>
                            <option value=""7"">7</option>
                            <option value=""6"">6</option>
                            <option value=""5"">5</option>
                            <option value=""4"">4</option>
                            <option value=""3"">3</option>
                            <option value=""2"">2</option>
                            <option value=""1"">1</option>                 
                        </select>
                        <label for=""divisionDropdown"" style=""color:#fff; margin-right:10px; margin-left:10px;"">Export Size:</label>
                        <select id=""divisionDropdown"" class=""giffy"">
                            <option value=""1"">100%</option>
                            <option value=""1.333"">75%</option>
                            <option value=""2"" selected>50%</option>
                            <option value=""4"">25%</option>                    
                        </select>
                       
                    <button class=""giffy"" id=""exportGif"" style=""margin-left:10px;"">Export GIF</button>
                    <button class=""giffy"" id=""closeEditor"" style=""margin-left:10px;"">Cancel</button>
                    </div> 
                    </div>
                `;
        
        
                document.body.appendChild(editor);
 
                const qualityDropdown = document.getElementById('qualityDropdown');
                const divisionDropdown = document.getElementById('divisionDropdown');
 
                const previewImage = document.getElementById('previewImage');
                const staticFrameImage = document.getElementById('staticFrameImage');
                const staticFrameCanvas = document.createElement('canvas');
                staticFrameCanvas.style.display = 'none';
                document.body.appendChild(staticFrameCanvas);
        
                const video = document.querySelector('video');
                staticFrameCanvas.width = video.videoWidth;
                staticFrameCanvas.height = video.videoHeight;
                const ctx = staticFrameCanvas.getContext('2d');
                ctx.drawImage(video, 0, 0, staticFrameCanvas.width, staticFrameCanvas.height);
                const staticFrameDataURL = staticFrameCanvas.toDataURL('image/webp');
                staticFrameImage.src = staticFrameDataURL;
        
                staticFrameImage.addEventListener('load', () => {
                    cropper = new Cropper(staticFrameImage, {
                        viewMode: 1,
                        autoCropArea: 1,
                        movable: true,
                        zoomable: true,
                        scalable: true,
                        rotatable: false,
                    });
                });
        
                const gifLengthSlider = document.getElementById('gifLengthSlider');
                noUiSlider.create(gifLengthSlider, {
                    start: [0, frames.length * FRAME_INTERVAL / 1000],
                    connect: true,
                    range: {
                        'min': 0,
                        'max': frames.length * FRAME_INTERVAL / 1000
                    }
                });
 
                gifLengthSlider.noUiSlider.on('update', (values) => {
                    const startFrame = Math.floor(values[0] * 1000 / FRAME_INTERVAL);
                    const endFrame = Math.floor(values[1] * 1000 / FRAME_INTERVAL);
                    lengthDisplay.textContent = `Start: ${values[0]}s (Frame ${startFrame}), End: ${values[1]}s (Frame ${endFrame})`;
                });
 
                gifLengthSlider.noUiSlider.on('end', (values) => {
                    updateGifPreview(Math.floor(values[0] * 10), Math.floor(values[1] * 10));
                });
        
                editor.querySelector('#exportGif').addEventListener('click', () => {
                    const length = gifLengthSlider.noUiSlider.get();
                    const quality = qualityDropdown.value;
                    const division = divisionDropdown.value;            
                    exportGif(length[0] * 10, length[1] * 10, cropper.getData(), quality, division);
                    document.body.removeChild(editor);
                    frames.length = 0;
                    recording = true;
                });
 
                editor.querySelector('#closeEditor').addEventListener('click', () => {
                    document.body.removeChild(editor);
                    frames.length = 0;
                    recording = true;
                });
        
                updateGifPreview(0, frames.length);
            }
 
            function updateGifPreview(start, end) {
                const loadingMessage = document.getElementById('loadingMessage');
                const previewImage = document.getElementById('previewImage');
                loadingMessage.style.display = 'block';
                previewImage.style.display = 'none';
    
                const framesToUse = frames.slice(start, end);
                if (framesToUse.length === 0) {
                    console.error(""Not enough frames to generate the GIF."");
                    loadingMessage.textContent = ""Error: Not enough frames."";
                    return;
                }
 
                const numbWorkers = navigator.hardwareConcurrency ? Math.min(navigator.hardwareConcurrency, 8) : 6;
 
                gifshot.createGIF({
                    images: framesToUse,
                    interval: FRAME_INTERVAL / 1000,
                    gifWidth: 530,
                    gifHeight: 298,
                    numWorkers: numbWorkers,
                    quality: 1,
                }, function (obj) {
                    if (!obj.error) {
                        const image = obj.image;
                        previewImage.src = image;
                        previewImage.style.display = 'block';
                        previewImage.style.bottom = '10px';
                        loadingMessage.style.display = 'none';
                    } else {
                        console.error('GIF generation failed:', obj.error);
                        loadingMessage.textContent = ""Error generating GIF."";
                    }
                });
            }
    
            function downloadBlob(blob, filename) {
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                URL.revokeObjectURL(url);
            }
 
            function exportGif(start, end, cropData, quality, division) {
                console.log(""Starting GIF export process with gifshot..."");
    
                const framesToUse = frames.slice(start, end).map((frameSrc) => {
                    return new Promise((resolve, reject) => {
                        const img = new Image();
                        img.src = frameSrc;
                        img.onload = () => {
                            const canvas = document.createElement('canvas');
                            const ctx = canvas.getContext('2d');
                            canvas.width = cropData.width;
                            canvas.height = cropData.height;
                            ctx.drawImage(img, cropData.x, cropData.y, cropData.width, cropData.height, 0, 0, cropData.width, cropData.height);
                            resolve(canvas.toDataURL());
                        };
                        img.onerror = (error) => {
                            console.error(`Failed to load frame:`, error);
                            reject(error);
                        };
                    });
                });
 
                const numbbWorkers = navigator.hardwareConcurrency ? Math.min(navigator.hardwareConcurrency, 8) : 6;
 
                Promise.all(framesToUse).then((croppedFrameUrls) => {
                    gifshot.createGIF({
                        'images': croppedFrameUrls,
                        'interval': FRAME_INTERVAL / 1000,
                        'gifWidth': cropData.width / division,
                        'gifHeight': cropData.height / division,
                        'numWorkers': numbbWorkers,
                        'quality': quality,
                    }, function (obj) {
                        if (!obj.error) {
                            const image = obj.image;
                            const downloadLink = document.createElement('a');
                            downloadLink.href = image;
                            downloadLink.download = 'fishtank.gif';
                            document.body.appendChild(downloadLink);
                            downloadLink.click();
                            document.body.removeChild(downloadLink);
                            console.log(""GIF successfully generated and downloaded."");
                        } else {
                            console.error(""GIF generation failed:"", obj.error);
                        }
                    });
                }).catch((error) => {
                    console.error('Error cropping frames:', error);
                });
            }
    
        })();
        ";


        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("fishtank.live.logo.ico"));

            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            this.Controls.Add(webView);

            InitializeTitleBar();

            webView.EnsureCoreWebView2Async(null).ContinueWith((task) =>
            {
                if (task.Exception != null)
                {
                    MessageBox.Show("WebView2 initialization failed.");
                }
                else
                {
                    webView.CoreWebView2.Navigate("https://www.fishtank.live/");

                    webView.CoreWebView2.NavigationCompleted += (s, e) =>
                    {
                        string script = userScript;
                        webView.ExecuteScriptAsync(script);
                    };
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            hoverTimer = new System.Windows.Forms.Timer();
            hoverTimer.Interval = 100;
            hoverTimer.Tick += HoverTimer_Tick;
            hoverTimer.Start();

            InitializeResizeHandle();
        }

        private void InitializeTitleBar()
        {
            titleBarPanel = new Panel();
            titleBarPanel.Dock = DockStyle.Top;
            titleBarPanel.Height = 15;
            titleBarPanel.BackColor = ColorTranslator.FromHtml("#191d21");

            Panel leftMarginSpacer = new Panel();
            leftMarginSpacer.Width = 10;
            leftMarginSpacer.Dock = DockStyle.Left;

            PictureBox iconBox = new PictureBox();
            iconBox.Width = 25;
            iconBox.Height = 15;
            iconBox.Dock = DockStyle.Left;
            iconBox.Margin = new Padding(15, 5, 0, 5); 
            iconBox.SizeMode = PictureBoxSizeMode.StretchImage;

            iconBox.Load("https://raw.githubusercontent.com/luna-mae/ClippingTools/refs/heads/main/media/logo2.png");

            Button closeButton = new Button();
            closeButton.Width = 30;
            closeButton.Height = 30;
            closeButton.Dock = DockStyle.Right;
            closeButton.BackColor = Color.Red;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 1;
            closeButton.FlatAppearance.BorderColor = Color.DarkRed;
            closeButton.Click += closeButton_Click;
            closeButton.Region = new Region(new System.Drawing.Drawing2D.GraphicsPath(new Point[] {
            new Point(15, 0), new Point(30, 15), new Point(15, 30), new Point(0, 15)
            }, new byte[] { 0x3, 0x3, 0x3, 0x3 }));

            Button fullscreenButton = new Button();
            fullscreenButton.Width = 30;
            fullscreenButton.Height = 30;
            fullscreenButton.Dock = DockStyle.Right;
            fullscreenButton.BackColor = Color.Green;
            fullscreenButton.FlatStyle = FlatStyle.Flat;
            fullscreenButton.FlatAppearance.BorderSize = 1;
            fullscreenButton.FlatAppearance.BorderColor = Color.DarkGreen;
            fullscreenButton.Click += (s, e) => ToggleFullscreen();
            fullscreenButton.Region = new Region(new System.Drawing.Drawing2D.GraphicsPath(new Point[] {
            new Point(15, 0), new Point(30, 15), new Point(15, 30), new Point(0, 15)
            }, new byte[] { 0x3, 0x3, 0x3, 0x3 }));

            Button minimizeButton = new Button();
            minimizeButton.Width = 30;
            minimizeButton.Height = 30;
            minimizeButton.Dock = DockStyle.Right;
            minimizeButton.BackColor = Color.Yellow;
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.FlatAppearance.BorderSize = 1;
            minimizeButton.FlatAppearance.BorderColor = Color.Goldenrod;
            minimizeButton.Click += minimizeButton_Click;
            minimizeButton.Region = new Region(new System.Drawing.Drawing2D.GraphicsPath(new Point[] {
            new Point(15, 0), new Point(30, 15), new Point(15, 30), new Point(0, 15)
            }, new byte[] { 0x3, 0x3, 0x3, 0x3 }));

            titleBarPanel.Controls.Add(iconBox);
            titleBarPanel.Controls.Add(leftMarginSpacer);
            titleBarPanel.Controls.Add(minimizeButton);
            titleBarPanel.Controls.Add(fullscreenButton);
            titleBarPanel.Controls.Add(closeButton);


            this.Controls.Add(titleBarPanel);

            titleBarPanel.MouseDown += titleBarPanel_MouseDown;
            titleBarPanel.MouseMove += titleBarPanel_MouseMove;
            titleBarPanel.MouseUp += titleBarPanel_MouseUp;
        }

        private void InitializeResizeHandle()
        {
            resizeHandle = new Label
            {
                Size = new Size(10, 10),
                BackColor = ColorTranslator.FromHtml("#740700"),
                Cursor = Cursors.SizeNWSE,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 10, this.ClientSize.Height - 10), 
            };
            resizeHandle.MouseDown += ResizeHandle_MouseDown;
            resizeHandle.MouseMove += ResizeHandle_MouseMove;
            resizeHandle.MouseUp += ResizeHandle_MouseUp;
            this.Controls.Add(resizeHandle);
            resizeHandle.BringToFront();
        }


        private void ResizeHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isFullscreen && e.Button == MouseButtons.Left)
            {
                isResizing = true;
                resizeOffset = new Point(e.X, e.Y);
            }
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                int newWidth = this.Width + (e.X - resizeOffset.X);
                int newHeight = this.Height + (e.Y - resizeOffset.Y);
                this.Size = new Size(newWidth, newHeight);
            }
        }

        private void ResizeHandle_MouseUp(object sender, MouseEventArgs e)
        {
            isResizing = false;
        }

        private void ToggleFullscreen()
        {
            if (isFullscreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                titleBarPanel.Visible = true;
                isTitleBarVisible = true;
                resizeHandle.Visible = true; 
                isFullscreen = false;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                isTitleBarVisible = false;
                titleBarPanel.Visible = false;
                resizeHandle.Visible = false;
                isFullscreen = true;
            }
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            if (isFullscreen)
            {
                Point mousePos = this.PointToClient(Cursor.Position);

                if (mousePos.Y <= 20 && !isTitleBarVisible)
                {
                    titleBarPanel.Visible = true;
                    isTitleBarVisible = true;
                }
                else if (mousePos.Y > 20 && isTitleBarVisible)
                {
                    titleBarPanel.Visible = false;
                    isTitleBarVisible = false;
                }
            }
            else
            {
                if (!titleBarPanel.Visible)
                {
                    titleBarPanel.Visible = true;
                }
            }
        }

        private void titleBarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            offset = new Point(e.X, e.Y);
        }

        private void titleBarPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - offset.X;
                this.Top += e.Y - offset.Y;
            }
        }

        private void titleBarPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
