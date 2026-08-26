// Variables Globales
let metodosPagoData = [];
let bancosData = [];
let tiposTarjetaData = [];
let nextPaymentIndex = 0;

const ID_EFECTIVO = "1";
const ID_TRANSFERENCIA = "2";
const ID_TARJETA = "3";
const ID_SISTEMA_CREDITO = "4";

const container = document.getElementById('paymentMethodsContainer');
const addBtn = document.getElementById('addPaymentMethod');
const template = document.getElementById('paymentMethodTemplate');
const emptyState = document.getElementById('emptyState');

const validarPrecioRestantePorPagar = document.getElementById('validarPrecioRestantePorPagar');
//const lblCantStockActual = document.getElementById('lblCantStockActual'); // cantidad en stock
//const cantUnidadesVender = document.getElementById('validarCantidadVendidos'); //cantidad a vender

//const inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
//const inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');
const inputPrecioTotalPagado = document.getElementById('validarPrecioTotalPagado');


let totalVentaProceso = 0;
let totalVentaPagadaProceso = 0;
let Max_Metodos_Pagos = 1;


document.addEventListener('DOMContentLoaded', async () => {
    
    await ValidarCantidadMetodosDePago();
    await cargarCatalogos();

    addBtn.addEventListener('click', () => {
        if (!ValidarMetodosDePagoCargados()) return;
        addPaymentMethodBlock(container, template, emptyState);
    });

    cantUnidadesVender.addEventListener("input", () => {
        CalcularVenta();
        ValidarStockCantidad();
        //calcularTotalPagado_Y_Restante(null, null, null);
    });

    inputPrecioVentaPorUni.addEventListener("input", () => {
        CalcularVenta();
    });


    async function cargarCatalogos() {
        try {
            const baseUrl = (window.location.origin === "null" || window.location.protocol === "blob:") ? "" : window.location.origin;

            // Usamos Promise.allSettled para que si un fetch falla, los demás puedan continuar
            const resultados = await Promise.allSettled([
                fetch(`${baseUrl}/Sis_MetodosDePagos/CargarMetodosDePago`).then(r => r.json()),
                fetch(`${baseUrl}/Sis_MetodosDePago_Bancos/CargarMetodosDePagoBancos`).then(r => r.json()),
                fetch(`${baseUrl}/Sis_MetodosDePago_TipoTarjetas/CargarMetodosDePagoTipoTarjeta`).then(r => r.json())
            ]);

            if (resultados[0].status === 'fulfilled') metodosPagoData = resultados[0].value;
            if (resultados[1].status === 'fulfilled') bancosData = resultados[1].value;
            if (resultados[2].status === 'fulfilled') tiposTarjetaData = resultados[2].value;

            // Mock Data de respaldo si los arrays vienen vacíos
            if (metodosPagoData.length === 0) {
                console.warn("Usando datos locales de respaldo.");
                metodosPagoData = [
                    { Value: "1", Text: "Efectivo" },
                    { Value: "2", Text: "Transferencia" },
                    { Value: "3", Text: "Tarjeta (Crédito/Débito)" },
                    { Value: "4", Text: "Crédito" }
                ];
                bancosData = [
                    { Value: "1", Text: "Bancolombia" },
                    { Value: "2", Text: "Davivienda" },
                    { Value: "3", Text: "Banco Bogotá" },
                    { Value: "4", Text: "BBVA" },
                    { Value: "5", Text: "NU" },                    
                    { Value: "6", Text: "DaviPlata" },                    
                    { Value: "7", Text: "Nequi" },
                    { Value: "8", Text: "BOLD" }                    
                ];
                tiposTarjetaData = [
                    { Value: "1", Text: "Crédito" },
                    { Value: "2", Text: "Débito" }
                ];
            }
        } catch (error) {
            console.error("Error crítico en catálogos:", error);
        }
    }

    async function addPaymentMethodBlock(container, template, emptyState) {
        const totalVenta = getTotalVenta();
        const totalVentaPagada = obtenerSumaTotalPagos();

        totalVentaProceso = totalVenta;
        totalVentaPagadaProceso = totalVentaPagada;

        const currentIndex = nextPaymentIndex;
        const clone = template.content.cloneNode(true);
        const block = clone.querySelector('.active-payment-block');
        const metodoSelect = block.querySelector('.metodo-pago-select');
        const bancoSelect = block.querySelector('.banco-select');
        const tipoTarjetaSelect = block.querySelector('.tipo-tarjeta-select');
        const divBanco = block.querySelector('.div-banco');
        const divTipoTarjeta = block.querySelector('.div-tipo-tarjeta');
        const removeBtn = block.querySelector('.remove-payment-btn');
        const inputMonto = block.querySelector('.monto-pago');


        // 2. REGLA: Validar si ya se alcanzó el total antes de intentar clonar
        if (totalVentaPagada >= totalVenta) {
            MostrarAlerta("warning", "Métodos de pago", "No se pueden agregar más métodos de pago. El total de venta ya está cubierto.", 10000, false, null);
            return;
        } else {
            // Reemplazar índices en los nombres
            block.querySelectorAll('[name*="[idx]"]').forEach(el => {
                el.name = el.name.replace('[idx]', `[${currentIndex}]`);
            });

            // Poblar Selects
            fillSelect(metodoSelect, metodosPagoData, "Metodos");
            fillSelect(bancoSelect, bancosData, "Bancos");
            fillSelect(tipoTarjetaSelect, tiposTarjetaData, "Tarjetas");

            metodoSelect.addEventListener('change', function () {
                const val = this.value;
                if (val === ID_EFECTIVO) {
                    setFieldState(divBanco, bancoSelect, false);
                    setFieldState(divTipoTarjeta, tipoTarjetaSelect, false);
                } else if (val === ID_TRANSFERENCIA) {
                    setFieldState(divBanco, bancoSelect, true);
                    setFieldState(divTipoTarjeta, tipoTarjetaSelect, false);
                } else if (val === ID_TARJETA) {
                    setFieldState(divBanco, bancoSelect, true);
                    setFieldState(divTipoTarjeta, tipoTarjetaSelect, true);
                } else if (val === ID_SISTEMA_CREDITO) {
                    setFieldState(divBanco, bancoSelect, false);
                    setFieldState(divTipoTarjeta, tipoTarjetaSelect, false);
                } else {
                    setFieldState(divBanco, bancoSelect, false);
                    setFieldState(divTipoTarjeta, tipoTarjetaSelect, false);
                }
            });

            setFieldState(divBanco, bancoSelect, false);
            setFieldState(divTipoTarjeta, tipoTarjetaSelect, false);

            inputMonto.addEventListener('input', function () {
                const valorIngresado = parseCurrency(this.value) || 0;

                let maximoPermitido = CalcularMaximoPermitido(totalVenta, totalVentaPagada);
                if (maximoPermitido === 0 || (valorIngresado <= maximoPermitido)) {
                    calcularTotalPagado_Y_Restante(totalVenta, totalVentaPagada, valorIngresado);
                } else {
                    this.value = maximoPermitido > 0 ? maximoPermitido : 0;
                    calcularTotalPagado_Y_Restante(totalVenta, totalVentaPagada, maximoPermitido);
                    MostrarAlerta("warning", "Métodos de pago", `El monto ingresado no puede exceder el valor de: ($${maximoPermitido}) pesos.`, 10000, false, null);
                }                
            });

            metodoSelect.addEventListener('change', () => {
                updateOptions();
            });

            removeBtn.addEventListener('click', () => {
                block.remove();
                checkEmptyState(container, emptyState);
                updateOptions();
                calcularTotalPagado_Y_Restante(totalVenta, totalVentaPagada, null);
            });

            container.appendChild(block);
            nextPaymentIndex++;
            checkEmptyState(container, emptyState);
            updateOptions();
            calcularTotalPagado_Y_Restante(totalVenta, totalVentaPagada, null);
        }
    }
            
                
});


function setFieldState(containerDiv, selectElement, isVisible) {
    if (isVisible) {
        containerDiv.classList.remove('hidden-field');
        selectElement.disabled = false;
        selectElement.required = true;
    } else {
        containerDiv.classList.add('hidden-field');
        selectElement.disabled = true;
        selectElement.required = false;
        selectElement.value = "";
    }
}

function fillSelect(selectElement, data, label) {
    const firstOption = selectElement.options[0];
    selectElement.innerHTML = '';
    if (firstOption) selectElement.appendChild(firstOption);

    if (!Array.isArray(data) || data.length === 0) {
        console.warn(`Data para ${label} está vacía.`);
        return;
    }

    data.forEach(item => {
        const opt = document.createElement('option');
        opt.value = item.Value || item.value || "";
        opt.textContent = item.Text || item.text || "Sin nombre";
        selectElement.appendChild(opt);
    });
}

function checkEmptyState(container, emptyState) {
    const count = container.querySelectorAll('.active-payment-block').length;
    if (emptyState) {
        emptyState.style.display = count === 0 ? 'block' : 'none';
    }
}



function validarBotonAgregarUI(totalVenta, totalVentaPagada) {
    if (!addBtn) return;

    totalVenta = totalVenta || 0;
    totalVentaPagada = totalVentaPagada || 0;

    if (totalVentaPagada <= totalVenta) {
        addBtn.disabled = false;
        addBtn.style.opacity = '1';
        addBtn.title = "Agregar Pago_0";
    } else {
        addBtn.disabled = true;
        addBtn.style.opacity = '0.5';
        addBtn.title = "Total de venta cubierto";
    }
}

function CalcularMaximoPermitido(totalVenta, totalVentaPagada) {
    const maximoPermitido = parseCurrency((totalVenta - totalVentaPagada));
    return parseFloat(maximoPermitido) || 0;
}

function parseCurrency(value) {
    if (!value) return 0;
    let cleanValue = value.toString().replace(/[^\d,]/g, '');
    cleanValue = cleanValue.replace(',', '.');
    return parseFloat(cleanValue) || 0;
}


// Validación de Stock
function ValidarStockCantidad() {
    const stock = parseInt(lblCantStockActual.getAttribute("data-valor-interno") || 0);
    const inputVender = parseInt(cantUnidadesVender.value) || 0;

    if (inputVender > stock) {
        cantUnidadesVender.value = stock; // asiganr el maximo de unidades disponibles
        MostrarAlerta("warning", "Stock Insuficiente", `Solo tienes ${stock} unidades disponibles. No puedes vender ${inputVender} unidades.`, 10000, false, "#3085d6");
        return;
    } else {
        AjustarPagos();
    }
}


function obtenerPagosRegistrados() {
    const contenedor = document.getElementById('paymentMethodsContainer');
    // Buscamos los elementos con la clase .payment-card que estén ADENTRO
    const bloques = contenedor.querySelectorAll('.payment-card');

    return Array.from(bloques).map((bloque, index) => {
        // Buscamos el input del monto y el select del método dentro de ese bloque
        const montoInput = bloque.querySelector('.monto-pago');
        const metodoSelect = bloque.querySelector('.metodo-pago-select');

        return {
            id: index, // Identificador basado en el orden
            elementoHtml: bloque, // Referencia al DOM por si necesitas borrarlo
            monto: parseFloat(montoInput.value) || 0,
            metodo: metodoSelect ? metodoSelect.value : 'No especificado'
        };
    });
}

function AjustarPagos() {
    const totalVenta = getTotalVenta();
    let pagosActuales = obtenerPagosRegistrados();
    let sumaPagos = pagosActuales.reduce((acc, pago) => acc + pago.monto, 0);

    // Mientras la suma de pagos sea mayor al nuevo total, eliminamos el último bloque del DOM
    while (sumaPagos > totalVenta && pagosActuales.length > 0) {
        const ultimoPago = pagosActuales.pop(); // Sacamos el último del array
        ultimoPago.elementoHtml.remove(); // LO ELIMINAMOS DEL HTML

        // Recalculamos la suma para el siguiente ciclo del bucle
        sumaPagos -= ultimoPago.monto;
        console.log(`Eliminado pago de ${ultimoPago.monto} por exceder total de ${totalVenta}`);
    }
    CalcularVenta();
    actualizarEstadoVacio();
}

function actualizarEstadoVacio() {
    const contenedor = document.getElementById('paymentMethodsContainer');
    const emptyState = document.getElementById('emptyState');

    if (contenedor.children.length === 0) {
        emptyState.classList.remove('d-none');
    } else {
        emptyState.classList.add('d-none');
    }
}


function updateOptions() {
    const selectedValues = getSelectedMethods();
    const allSelects = container.querySelectorAll('.metodo-pago-select');

    allSelects.forEach(select => {
        const currentValue = select.value;
        Array.from(select.options).forEach(option => {
            if (option.value === "") return;

            // Deshabilitar si ya está seleccionado en OTRO select
            const isSelectedElsewhere = selectedValues.includes(option.value) && option.value !== currentValue;
            option.disabled = isSelectedElsewhere;

            // Estética opcional
            option.style.color = isSelectedElsewhere ? '#ccc' : '#000';
        });
    });
}

function getSelectedMethods() {
    const selects = container.querySelectorAll('.metodo-pago-select');
    return Array.from(selects).map(s => s.value).filter(v => v !== "");
}


async function ValidarCantidadMetodosDePago() {
    try {
        const response = await fetch(`/Funciones/CantidadMetodosPago`);
        if (response.ok) {
            const data = await response.text();
            const parsed = parseInt(data);
            if (!isNaN(parsed)) {
                Max_Metodos_Pagos = parsed;
            }
        }
    } catch (error) {
        console.warn("No se pudo obtener el límite del servidor, usando valor por defecto:", Max_Metodos_Pagos);
    }
}

function ValidarMetodosDePagoCargados() {
    const blocks = container.querySelectorAll('.active-payment-block');

    // Regla 1: Límite máximo
    if (blocks.length >= Max_Metodos_Pagos) {
        MostrarAlerta("info", "Métodos de pago", "Ya has utilizado todos los métodos de pago disponibles.", 10000, false, null);
        return false;
    }

    let isValid = true;
    blocks.forEach(block => {
        const select = block.querySelector('.metodo-pago-select');
        const input = block.querySelector('.monto-pago');
        const montoValue = parseFloat(input.value) || 0;

        // Regla 2: Método seleccionado y monto > 0
        if (select.value === "" || montoValue <= 0) {
            isValid = false;
            select.classList.toggle('invalid-field', select.value === "");
            input.classList.toggle('invalid-field', montoValue <= 0);
        } else {
            select.classList.remove('invalid-field');
            input.classList.remove('invalid-field');
        }
    });

    if (!isValid) {
        MostrarAlerta("info", "Métodos de pago", "Por favor, complete correctamente el método y el monto de los pagos actuales antes de agregar uno nuevo.", 10000, false, null);
    }
    return isValid;
}


function SoloFormatoMoneda(valor) {
    if (!isNaN(valor)) {
        // Formatear el valor como moneda COP
        const formatoMoneda = new Intl.NumberFormat("es-CO", {
            style: "currency",
            currency: "COP",
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });
        const cantidadFormateada = formatoMoneda.format(valor);
        return valor = cantidadFormateada;
    }
}

function QuitarFormatoMoneda(valorFormateado) {
    if (!valorFormateado) return 0;

    // Convertir a string por si llega un número
    const texto = String(valorFormateado);

    // Eliminar todo lo que no sea un dígito numérico
    const numeroLimpio = texto.replace(/\D/g, '');

    // Retornar como número entero (o 0 si la cadena estaba vacía)
    return parseInt(numeroLimpio, 10) || 0;
}

function MostrarAlerta(icon, title, text, timer, showConfirmButton, confirmButtonColor) {
    Swal.fire({
        icon: icon,
        title: title,
        text: text,
        timer: timer,
        showConfirmButton: showConfirmButton,
        confirmButtonColor: confirmButtonColor
    });
}


function calcularTotalPagado_Y_Restante(totalVenta, totalVentaPagada, ingresado) {

    if (totalVenta == null) {
        totalVenta = getTotalVenta();
    }

    if (totalVentaPagada == null) {
        totalVentaPagada = obtenerSumaTotalPagos();
    }

    totalVenta = totalVenta || 0;
    totalVentaPagada = totalVentaPagada || 0;
    ingresado = ingresado || 0;

    //Total pagado
    if (inputPrecioTotalPagado) {
        let result = SoloFormatoMoneda(totalVentaPagada + ingresado);
        inputPrecioTotalPagado.value = result;
    }

    //Saldo restante
    if (validarPrecioRestantePorPagar) {
        let saldoRestante = CalcularMaximoPermitido(totalVenta, totalVentaPagada);
        saldoRestante = saldoRestante - ingresado;
        validarPrecioRestantePorPagar.value = SoloFormatoMoneda(saldoRestante);
    }
    validarBotonAgregarUI();
}


const getTotalVenta = () => {
    const totalVenta = document.getElementById('lblTotalPagar');
    return totalVenta ? parseCurrency(totalVenta.value) || 0 : 0;
};

function obtenerSumaTotalPagos() {
    let sumaTodos = 0;
    container.querySelectorAll('.monto-pago').forEach(inp => {
        if (inp !== this) sumaTodos += parseCurrency(inp.value) || 0;
    });
    return parseCurrency(sumaTodos);
}