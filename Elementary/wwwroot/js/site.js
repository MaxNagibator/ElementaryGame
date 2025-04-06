let currentPage = 1;
let currentQuestionNumber = 1;

function init() {
    refreshPage();
}

function changePage(page) {
    currentPage = page;
    refreshPage();
}

function refreshPage() {
    document.querySelectorAll(".game-page").forEach(element => element.classList.add('hidden'));
    document.getElementById('page-' + currentPage).classList.remove('hidden');

    if (currentPage === 1) {
        document.body.classList.add('page-1-background');
    } else {
        document.body.classList.remove('page-1-background');
    }
}

game = {};

function setMode(val) {
    game.mode = val;
    changePage(2);
}

function setSkin(val) {
    changePage(3);
    startGame();
}

function setLevel(val) {
    game.level = val;
    loadQuestion();
}

function startGame() {
    SendRequest({
        method: 'POST',
        url: '/Home/StartGame',
        body: {
            playerSecret: "test"
        },
        success(data) {
            currentQuestionNumber = 1;
            loadQuestion();
        }
    });
}

function nextQuestion() {
    const confirmBtn = document.querySelector('.confirm-btn');
    if (confirmBtn) {
        confirmBtn.remove();
    }

    const explanationContainer = document.getElementById('Explanation');
    explanationContainer.classList.remove('visible');
    explanationContainer.innerHTML = '';

    document.querySelectorAll('.text-input').forEach(input => {
        input.value = '';
        input.removeAttribute('readonly');
        input.classList.remove('correct-answer', 'wrong-answer');
    });

    document.querySelectorAll('.option-btn')
        .forEach(btn => btn.classList.remove('active', 'correct-answer', 'disabled'));

    currentQuestionNumber++;
    loadQuestion();
}

function loadQuestion() {
    SendRequest({
        method: 'POST',
        url: '/Home/GetNextQuestion',
        body: {
            questionNumber: currentQuestionNumber,
            level: game.level
        },
        success(data) {
            const question = JSON.parse(data.responseText);
            renderQuestion(question);
            changePage(4);
        }
    });
}

function renderQuestion(question) {
    const block = document.getElementById('QuestionBlock');
    block.innerHTML = `
        <h3>Вопрос ${currentQuestionNumber}</h3>
        <div class="question-text">${question.text}</div>
        ${question.type === 'text' ?
        `<div class="text-inputs-container">
            ${Array.from({ length: 4 }, (_, i) => `
            <input type="text" class="text-input" maxlength="1" data-index="${i}">
        `).join('')}
        </div>` :
        `<div class="options-table">${question.options.map(o => `
                <div class="option-btn">${o}</div>
            `).join('')}</div>`
    }
    `;

    const answerContainer = document.getElementById('AnswerBlock');
    const confirmBtn = document.createElement('button');
    confirmBtn.className = 'btn btn-info confirm-btn';
    confirmBtn.textContent = 'Подтвердить ответ';
    confirmBtn.disabled = true;
    confirmBtn.onclick = submitAnswer;
    answerContainer.appendChild(confirmBtn);

    const inputs = document.querySelectorAll('.text-input');
    const optionBtns = document.querySelectorAll('.option-btn');

    if (question.type === 'text') {
        inputs.forEach((input, index) => {
            input.addEventListener('input', e => {
                if (e.target.value.length === 1) {
                    if (index < inputs.length - 1) {
                        inputs[index + 1].focus();
                    }
                }
                const allFilled = Array.from(inputs).every(i => i.value.trim() !== '');
                confirmBtn.disabled = !allFilled;
            });

            input.addEventListener('keydown', e => {
                if (e.key === 'Backspace' && !e.target.value) {
                    if (index > 0) {
                        inputs[index - 1].focus();
                    }
                }
            });
        });
    } else if (question.type === 'multiple') {
        optionBtns.forEach(btn =>
            btn.addEventListener('click', function () {
                if (!this.classList.contains('disabled')) {
                    document.querySelectorAll('.option-btn').forEach(b => b.classList.remove('active'));
                    this.classList.add('active');
                }
                confirmBtn.disabled = false;
            }));
    }
}


function submitAnswer() {
    const inputs = document.querySelectorAll('.text-input');
    const selectedOption = document.querySelector('.option-btn.active');
    const confirmBtn = document.querySelector('.confirm-btn');
    confirmBtn.disabled = true;
    confirmBtn.textContent = 'Ответ принят';

    let answer;

    if (inputs.length > 0) {
        answer = Array.from(inputs).map(input => input.value.trim()).join('').toUpperCase();
        inputs.forEach(input => input.setAttribute('readonly', true));
    } else if (selectedOption) {
        answer = selectedOption.innerText;
        document.querySelectorAll('.option-btn').forEach(btn => btn.classList.add('disabled'));
    }

    SendRequest({
        method: 'POST',
        url: '/Home/SetAnswer',
        body: {
            value: answer
        },
        success(data) {
            const result = JSON.parse(data.responseText);
            const isCorrect = answer === result.answer.toUpperCase();

            if (inputs.length > 0) {
                inputs.forEach(input => input.classList.add(isCorrect ? 'correct-answer' : 'wrong-answer'));
            } else {
                document.querySelectorAll('.option-btn').forEach(btn => {
                    if (btn.innerText === result.answer) {
                        btn.classList.add('correct-answer');
                    }
                });
            }

            if (result.imageUrl) {
                const explanationContainer = document.getElementById('Explanation');
                explanationContainer.classList.add('visible');

                const explanationImage = document.createElement('img');
                explanationImage.src = result.imageUrl;
                explanationImage.className = 'explanation-image';
                explanationContainer.appendChild(explanationImage);

                setTimeout(() => {
                    const rect = explanationContainer.getBoundingClientRect();
                    if (rect.top < 0 || rect.bottom > window.innerHeight) {
                        explanationContainer.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                            inline: 'nearest'
                        });
                    }
                }, 150);
            }

            setTimeout(() => {
                confirmBtn.textContent = 'Следующий вопрос';
                confirmBtn.onclick = nextQuestion;
                confirmBtn.disabled = false;
            }, 500);
        }
    });
}

document.addEventListener('DOMContentLoaded', init);


var currentAngle = 0;

function spinWheel() {
    const sectorValue = getRandomInt(1, 12);
    // 360 градусов это 12 сектаров, значит 1 сектор это 360 / 12 = 30 градусов
    const sector = 30;

    ruletkaDiv = document.getElementById('ruletka');

    var rounds = getRandomInt(2, 5);
    const time = getRandomInt(3, 8);
    angle = rounds * 360 + sectorValue * sector;
    currentAngle += angle;
    ruletkaDiv.style.transition = `transform ${time}s cubic-bezier(0.1, 0.7, 0.1, 1)`;
    ruletkaDiv.style.transform = `rotate(${currentAngle}deg)`;
}

function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function SendRequest(options) {
    let _this = {};
    let defaultOptions = {
        method: 'POST',
    };

    _this.options = Object.assign({}, defaultOptions, options);

    _this.Send = function () {
        let xhr = new XMLHttpRequest();
        xhr.open(_this.options.method, _this.options.url, true);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        xhr.onreadystatechange = function () {
            if (this.readyState != 4) {
                return;
            }
            if (this.status == 200) {
                if (_this.options.success) {
                    _this.options.success(this);
                }
            } else if (this.status == 403) {
                showAlert('Внимание', this.responseText, 5);
            } else {
                if (_this.options.error) {
                    _this.options.error(this);
                } else {
                    showAlert('Ошибка', 'чтото пошло не так', 2);
                }
            }
            if (_this.options.always) {
                _this.options.always(this);
            }
        };
        xhr.send(JSON.stringify(_this.options.body));
    };
    _this.Send();
}

var globalAlertId = 0;

function showAlert(title, message, timeoutSeconds) {
    globalAlertId++;
    let alertId = globalAlertId;
    if (!document.getElementById('userAlertsBody')) {
        let alertsBody = document.createElement('div');
        alertsBody.id = 'userAlertsBody';
        alertsBody.classList.add('alerts-body');
        document.body.append(alertsBody);
    }

    let alert = document.createElement('div');
    alert.id = 'alert-' + alertId;
    alert.classList.add('alert');
    alert.classList.add('alert-warning');
    document.getElementById('userAlertsBody').append(alert);
    alert.innerHTML =
        "    <a class='close' href='#' onclick='hideAlert(this)'>X</a>"
        + "    <h4></span>" + title + "</h4>"
        + "    <label>" + message + "</label>";

    if (timeoutSeconds) {
        setTimeout(function () {
            hideAlertById(alert.id);
        }, timeoutSeconds * 1000);
    }
}

function hideAlertById(alertId) {
    document.getElementById(alertId).remove();
}

function hideAlert(elem) {
    let alertBlock = elem.closest('.alert');
    alertBlock.remove();
}
