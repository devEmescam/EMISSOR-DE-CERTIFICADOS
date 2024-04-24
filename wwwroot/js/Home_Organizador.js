window.onload = function () {
    mostrarCard('meus-eventos');
};

function mostrarCard(id) {
    var cards = document.querySelectorAll('.card');
    cards.forEach(function (card) {
        card.style.display = 'none';
    });

    var cardToShow = document.getElementById('card-' + id);
    if (cardToShow) {
        cardToShow.style.display = 'block';
    }
}

function exibirFormularioAdicionar() {
    var cards = document.querySelectorAll('.card');
    cards.forEach(function (card) {
        card.style.display = 'none';
    });

    var cardNovoEvento = document.getElementById('card-evento');
    if (cardNovoEvento) {
        cardNovoEvento.style.display = 'block';
        cardNovoEvento.scrollIntoView({ behavior: 'smooth' });
    }
}

var campoPesquisa = document.getElementById('campo-pesquisa');
var tabelaEventos = document.querySelector('.tabela-eventos');
var linhasEventos = tabelaEventos.getElementsByTagName('tr');

campoPesquisa.addEventListener('input', function () {
    var termoPesquisa = campoPesquisa.value.toLowerCase();

    for (var i = 1; i < linhasEventos.length; i++) {
        var colunasEvento = linhasEventos[i].getElementsByTagName('td');
        var encontrado = false;

        for (var j = 0; j < colunasEvento.length; j++) {
            var textoColuna = colunasEvento[j].textContent || colunasEvento[j].innerText;
            if (textoColuna.toLowerCase().indexOf(termoPesquisa) > -1) {
                encontrado = true;
                break;
            }
        }

        if (encontrado) {
            linhasEventos[i].style.display = '';
        } else {
            linhasEventos[i].style.display = 'none';
        }
    }
});

var modoAdicionar = true;

function alternarModo() {
    modoAdicionar = !modoAdicionar;
    var botaoAcao = document.getElementById("botaoAcao");
    if (modoAdicionar) {
        botaoAcao.textContent = "Adicionar Dados";
    } else {
        botaoAcao.textContent = "Atualizar Dados";
    }
}

function limparCampos() {
    document.getElementById("inputNome").value = "";
    document.getElementById("inputCPF").value = "";
    document.getElementById("inputEmail1").value = "";
    document.getElementById("tipoPessoa").value = "";
    document.getElementById("textareaTexto").value = "";
}

function carregarDadosParaEditar(linha) {
    var celulas = linha.getElementsByTagName("td");
    var inputNome = document.getElementById("inputNome");
    var inputCPF = document.getElementById("inputCPF");
    var inputEmail = document.getElementById("inputEmail1");
    var inputTipoPessoa = document.getElementById("tipoPessoa");
    var textareaTexto = document.getElementById("textareaTexto");

    inputNome.value = celulas[0].innerHTML;
    inputCPF.value = celulas[1].innerHTML;
    inputEmail.value = celulas[2].innerHTML;
    inputTipoPessoa.value = celulas[3].innerHTML;
    textareaTexto.value = celulas[4].innerHTML;

    linhaEditavel = linha;
    alternarModo();
}

var botaoCadastrarEvento = document.getElementById('CadastrarEvento');

botaoCadastrarEvento.addEventListener('click', function () {
    var nomeEvento = document.getElementById('nomeEvento').value;
    if (nomeEvento.trim() === '') {
        alert('Por favor, insira o nome do evento.');
        return;
    }

    var tabelaEventos = document.querySelector('.tabela-eventos table');
    var novaLinha = tabelaEventos.insertRow(-1);
    var celulaEvento = novaLinha.insertCell(0);
    celulaEvento.textContent = nomeEvento;
});

function importarExcel() {
    var input = document.getElementById('excelFile');
    if (!input.files || input.files.length === 0) {
        alert('Por favor, selecione um arquivo Excel.');
        return;
    }

    var file = input.files[0];
    var reader = new FileReader();

    reader.onload = function (e) {
        var data = new Uint8Array(e.target.result);
        var workbook = XLSX.read(data, { type: 'array' });
        var sheetName = workbook.SheetNames[0];
        var worksheet = workbook.Sheets[sheetName];
        var expectedColumns = ['NOME_PESSOA', 'CPF_PESSOA', 'EMAIL_PESSOA', 'TIPO_PESSOA', 'TEXTO'];
        var headerRow = XLSX.utils.sheet_to_json(worksheet, { header: 1, range: 'A1:E1' })[0];
        var isValidModel = expectedColumns.every(function (col, index) {
            return headerRow[index] === col;
        });

        if (!isValidModel) {
            alert('O modelo da planilha selecionada está incorreto.');
            return;
        }

        var table = document.getElementById('tabela').getElementsByTagName('tbody')[0];
        var rows = XLSX.utils.sheet_to_json(worksheet, { header: 1, range: 1 });

        rows.forEach(function (row) {
            var newRow = table.insertRow(-1);
            row.forEach(function (cellContent) {
                var cell = newRow.insertCell();
                cell.textContent = cellContent;
            });
            adicionarBotoesLinha(newRow);
        });
    };

    reader.readAsArrayBuffer(file);
}

function adicionarBotoesLinha(linha) {
    var colunaAcoes = linha.insertCell(-1);
    var botaoExcluir = document.createElement("button");
    botaoExcluir.textContent = "Excluir";
    botaoExcluir.onclick = function () {
        excluirLinha(linha);
    };
    colunaAcoes.appendChild(botaoExcluir);

    var botaoAtualizar = document.createElement("button");
    botaoAtualizar.textContent = "Atualizar";
    botaoAtualizar.onclick = function () {
        carregarDadosParaEditar(linha);
    };
    colunaAcoes.appendChild(botaoAtualizar);
}

function excluirLinha(linha) {
    var index = linha.rowIndex;
    document.getElementById("tabela").deleteRow(index);
}

function atualizarDados() {
    if (modoAdicionar) {
        var tabela = document.getElementById("tabela");
        var nome = document.getElementById("inputNome").value;
        var cpf = document.getElementById("inputCPF").value;
        var email = document.getElementById("inputEmail1").value;
        var tipoPessoa = document.getElementById("tipoPessoa").value;
        var texto = document.getElementById("textareaTexto").value;

        if (nome === "" || cpf === "" || email === "" || tipoPessoa === "" || texto === "") {
            alert("Por favor, preencha todos os campos.");
            return;
        }

        var novaLinha = tabela.insertRow(-1);
        var celulaNome = novaLinha.insertCell(0);
        var celulaCPF = novaLinha.insertCell(1);
        var celulaEmail = novaLinha.insertCell(2);
        var celulaTipoPessoa = novaLinha.insertCell(3);
        var celulaTexto = novaLinha.insertCell(4);

        celulaNome.innerHTML = nome;
        celulaCPF.innerHTML = cpf;
        celulaEmail.innerHTML = email;
        celulaTipoPessoa.innerHTML = tipoPessoa;
        celulaTexto.innerHTML = texto;

        adicionarBotoesLinha(novaLinha);
        limparCampos();
    } else {
        var celulas = linhaEditavel.getElementsByTagName("td");
        celulas[0].innerHTML = document.getElementById("inputNome").value;
        celulas[1].innerHTML = document.getElementById("inputCPF").value;
        celulas[2].innerHTML = document.getElementById("inputEmail1").value;
        celulas[3].innerHTML = document.getElementById("tipoPessoa").value;
        celulas[4].innerHTML = document.getElementById("textareaTexto").value;
        alternarModo();
    }
}
