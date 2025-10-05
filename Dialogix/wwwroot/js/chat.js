document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".chat-form");
    const input = document.getElementById("UserInput");
    const messagesList = document.getElementById("messagesList");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const message = input.value.trim();
        if (!message) return;

       
        addMessage("You", message);

        
        const typingIndicator = addTypingIndicator();

        try {
            const response = await fetch("/Chat?handler=SendMessage", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ text: message })
            });

            if (response.ok) {
                const data = await response.json();
                removeTypingIndicator(typingIndicator);
                addMessage("Bot", data.response);
            } else {
                removeTypingIndicator(typingIndicator);
                addMessage("Bot", "Ошибка при отправке сообщения");
            }
        } catch (err) {
            console.error(err);
            removeTypingIndicator(typingIndicator);
            addMessage("Bot", "Ошибка при отправке сообщения");
        }

        input.value = "";
        messagesList.scrollTop = messagesList.scrollHeight;
    });

    function addMessage(user, text) {
        const li = document.createElement("li");
        li.className = `message ${user === "Bot" ? "bot" : "user"}`;
        li.innerHTML = `<div class="bubble">
                            <div class="meta">
                                <span class="user">${user}</span>
                                <span class="time">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                            </div>
                            <div class="text">${text}</div>
                        </div>`;
        messagesList.appendChild(li);
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    function addTypingIndicator() {
        const li = document.createElement("li");
        li.className = "message bot typing";
        li.innerHTML = `<div class="bubble">
                            <div class="meta">
                                <span class="user">Bot</span>
                                <span class="time">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                            </div>
                            <div class="text">Bot is typing<span class="dots">...</span></div>
                        </div>`;
        messagesList.appendChild(li);
        messagesList.scrollTop = messagesList.scrollHeight;
        return li;
    }

    function removeTypingIndicator(li) {
        if (li && messagesList.contains(li)) {
            messagesList.removeChild(li);
        }
    }
});
