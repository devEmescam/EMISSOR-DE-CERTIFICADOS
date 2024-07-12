
// Função para verificar a sessão periodicamente
function checkSession() {
    console.log('checkSession called'); // Log de depuração
    $.ajax({
        url: '/Home_Organizador/CheckSession', // Você precisará de uma ação no controller que verifica a sessão
        type: 'GET',
        success: function (data) {
            // Sessão está ativa, não fazer nada
        },
        error: function (xhr, status, error) {
            if (xhr.status === 401) {
                window.location.href = "/home/index";
            }
        }
    });
}

// Verificar a sessão a cada 1 minuto
setInterval(checkSession, 300000); // 300000 ms = 30 min

// Manipulador global para erros AJAX
$(document).ajaxError(function (event, xhr, settings, thrownError) {
    if (xhr.status === 401) {
        window.location.href = "/home/index";
    }
});
