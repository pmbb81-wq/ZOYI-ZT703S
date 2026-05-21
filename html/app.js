(function () {
    'use strict';

    const DOM = {
        label: document.getElementById('label'),
        value: document.getElementById('value'),
        unit: document.getElementById('unit'),
        mode1: document.getElementById('mode1'),
        mode2: document.getElementById('mode2'),
        freq: document.getElementById('freq'),
        freqUnit: document.getElementById('freqUnit'),
        capUnit: document.getElementById('capUnit'),
        statusDot: document.getElementById('statusDot'),
        statusText: document.getElementById('statusText'),
    };

    let evtSource = null;
    let reconnectTimer = null;
    const RECONNECT_DELAY = 3000;

    function setStatus(state) {
        DOM.statusDot.className = 'status-dot';
        switch (state) {
            case 'connected':
                DOM.statusDot.classList.add('connected');
                DOM.statusText.textContent = 'Connected';
                break;
            case 'connecting':
                DOM.statusText.textContent = 'Connecting...';
                break;
            case 'error':
                DOM.statusDot.classList.add('error');
                DOM.statusText.textContent = 'Reconnecting...';
                break;
            default:
                DOM.statusText.textContent = 'Disconnected';
        }
    }

    function updateDisplay(data) {
        if (data.Label) DOM.label.textContent = data.Label;
        if (data.Value) DOM.value.textContent = data.Value;
        if (data.Unit) DOM.unit.textContent = data.Unit;
        if (data.Mode1) DOM.mode1.textContent = data.Mode1 || '--';
        if (data.Mode2) DOM.mode2.textContent = data.Mode2 || '--';
        if (data.Freq) DOM.freq.textContent = data.Freq;
        if (data.Freq_unit) DOM.freqUnit.textContent = data.Freq_unit;
        if (data.Cap_unit) DOM.capUnit.textContent = data.Cap_unit || '--';
    }

    function connect() {
        if (evtSource) {
            evtSource.close();
            evtSource = null;
        }

        setStatus('connecting');

        evtSource = new EventSource('/measure');

        evtSource.onopen = function () {
            setStatus('connected');
        };

        evtSource.onmessage = function (event) {
            try {
                const data = JSON.parse(event.data);
                updateDisplay(data);
            } catch (e) {
                console.error('Failed to parse SSE data:', e);
            }
        };

        evtSource.onerror = function () {
            setStatus('error');
            evtSource.close();
            evtSource = null;
            reconnectTimer = setTimeout(connect, RECONNECT_DELAY);
        };
    }

    function cleanup() {
        if (reconnectTimer) {
            clearTimeout(reconnectTimer);
            reconnectTimer = null;
        }
        if (evtSource) {
            evtSource.close();
            evtSource = null;
        }
    }

    window.addEventListener('beforeunload', cleanup);
    connect();
})();
