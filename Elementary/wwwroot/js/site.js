var currentPage = 1;

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
}

document.addEventListener("DOMContentLoaded", function () {
    init();
});

game = {};

function setMode(val) {
    game.mode = val;
    changePage(2);
}

function setSkin(val) {
    changePage(3);
}

function setLevel(val) {
    game.level = val;
    changePage(4);
}

function startGame() {
    SendRequest({
        method: 'POST',
        url: '/Home/StartGame',
        body: {
            playerSecret: "test"
        },
        success(data) {
        }
    });
}

function getQuestion() {
    SendRequest({
        method: 'POST',
        url: '/Home/GetNextQuestion',
        body: {
            playerSecret: "test"
        },
        success(data) {
            let question = JSON.parse(data.responseText);
            block = document.getElementById('QuestionBlock');
            block.innerHTML = "";

            const textDiv = document.createElement('div');
            textDiv.innerHTML = question.text;
            block.appendChild(textDiv);

            if (question.options) {
                const optionsDiv = document.createElement('div');
                let text = "<div class='options-table'>";
                for (let i = 0; i < question.options.length; i++) {
                    text += "<div class='option-btn'>" + question.options[i] + "</div>"
                }
                text + "</div>";

                optionsDiv.innerHTML = text;
                block.appendChild(optionsDiv);

                document.querySelectorAll(".option-btn").forEach(element =>
                    element.addEventListener('click', function (event) {
                        document.querySelectorAll(".option-btn").forEach(element => element.classList.remove('active'));

                        event.target.classList.add('active');
                    }));
            }

            const confirmButton = document.createElement('button');
            confirmButton.classList.add('confirm-btn', 'btn', 'btn-info');
            confirmButton.innerHTML = "подтвердить ответ";
            confirmButton.addEventListener('click', function (event) {
                let activeBtns = document.querySelectorAll('#QuestionBlock .option-btn.active');
                if (activeBtns.length > 0) {
                    answer = activeBtns[0].innerHTML;
                    sendAnswer(answer);
                }
            });
            block.appendChild(confirmButton);

        }
    });
}

function sendAnswer(answer) {
    SendRequest({
        method: 'POST',
        url: '/Home/SetAnswer',
        body: {
            value: answer
        },
        success(data) {
            let answer = JSON.parse(data.responseText);
            let btns = document.querySelectorAll('#QuestionBlock .option-btn');
            for (let i = 0; i < btns.length; i++) {
                if (btns[i].innerHTML == answer.answer) {
                    btns[i].classList.add('right-answer')
                }
            }

            let confirmBtn = document.querySelectorAll('#QuestionBlock .confirm-btn');
            confirmBtn[0].remove();

            const explanationDiv = document.createElement('div');
            explanationDiv.classList.add('explanation');
            explanationDiv.innerHTML = answer.explanation;

            block = document.getElementById('QuestionBlock');
            block.appendChild(explanationDiv);

        }
    });
}


var currentAngle = 0;

function ruletkaStart() {
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
