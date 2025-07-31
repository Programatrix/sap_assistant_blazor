window.initCarousel = () => {
    const slides = document.querySelectorAll(".carousel img");
    let index = 0;

    setInterval(() => {
        slides[index].classList.remove("active");
        index = (index + 1) % slides.length;
        slides[index].classList.add("active");
    }, 3000);
};

window.initScrollBtn = () => {
    const btn = document.getElementById("scrollTopBtn");
    window.addEventListener("scroll", () => {
        btn.style.display = window.scrollY > 200 ? "block" : "none";
    });
};

document.addEventListener('DOMContentLoaded', () => {
    const chatBox = document.getElementById('chat-box');
    const userInput = document.getElementById('user-input');
    const sendBtn = document.getElementById('send-btn');

    // Verificación rápida
    console.log("Chat Playground inicializado:", chatBox, userInput, sendBtn);

    function addMessage(sender, text) {
        const div = document.createElement('div');
        div.className = `chat-bubble ${sender === 'Usuario' ? 'user' : 'bot'}`;
        div.textContent = text;
        chatBox.appendChild(div);
        chatBox.scrollTop = chatBox.scrollHeight;
    }

    function handleSend() {
        const message = userInput.value.trim();
        if (!message) return;

        addMessage('Usuario', message);
        userInput.value = '';

        // Simulación de respuesta del bot
        setTimeout(() => {
            addMessage('Bot', 'Procesando tu solicitud... 📊');
        }, 1000);
    }

    // Click en botón
    sendBtn.addEventListener('click', handleSend);

    // Enter para enviar
    userInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') handleSend();
    });
});

