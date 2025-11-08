// AidCircle Chat Utilities - Microphone and Speech Recognition

window.aidCircleChat = {
    // Microphone recording state
    mediaRecorder: null,
    audioChunks: [],
    
    /**
     * Initialize microphone access and start recording
     * @returns {Promise<boolean>} Success status
     */
    async startRecording() {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);
            this.audioChunks = [];

            this.mediaRecorder.ondataavailable = (event) => {
                this.audioChunks.push(event.data);
            };

            this.mediaRecorder.start();
            console.log('Recording started');
            return true;
        } catch (error) {
            console.error('Error accessing microphone:', error);
            return false;
        }
    },

    /**
     * Stop recording and return audio blob
     * @returns {Promise<string>} Base64 encoded audio data
     */
    async stopRecording() {
        return new Promise((resolve, reject) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === 'inactive') {
                reject('No active recording');
                return;
            }

            this.mediaRecorder.onstop = async () => {
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
                const base64Audio = await this.blobToBase64(audioBlob);
                
                // Stop all tracks
                this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
                
                resolve(base64Audio);
            };

            this.mediaRecorder.stop();
            console.log('Recording stopped');
        });
    },

    /**
     * Convert Blob to Base64
     * @param {Blob} blob 
     * @returns {Promise<string>}
     */
    blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result.split(',')[1]);
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    },

    /**
     * Check if browser supports microphone
     * @returns {boolean}
     */
    isMicrophoneSupported() {
        return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
    },

    /**
     * Scroll chat container to bottom
     * @param {string} elementId 
     */
    scrollToBottom(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    },

    /**
     * Auto-resize textarea based on content
     * @param {HTMLElement} element 
     */
    autoResizeTextarea(element) {
        element.style.height = 'auto';
        element.style.height = element.scrollHeight + 'px';
    },

    /**
     * Copy text to clipboard
     * @param {string} text 
     * @returns {Promise<boolean>}
     */
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (error) {
            console.error('Failed to copy:', error);
            return false;
        }
    },

    /**
     * Show toast notification
     * @param {string} message 
     * @param {string} type - 'success', 'error', 'info'
     */
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `ac-toast ac-toast-${type}`;
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            bottom: 2rem;
            right: 2rem;
            padding: 1rem 1.5rem;
            background: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#2563eb'};
            color: white;
            border-radius: 0.5rem;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }
};

// Add animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
