// Variable global o de ámbito de la venta
let lotesActuales = null;
let lotesSeleccionados = []; // Guarda los IDs de los lotes activos en orden de clic
let precioVentaBaseOriginal = 0;
let ImagenProducto = document.getElementById('ImagenProducto');
let previewImagenProducto = document.getElementById('previewImagenProducto');
// CORREGIDO: Se declara la variable a nivel global para que sea accesible en todo el script
const iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
const validarCodigo = document.getElementById("validarCodigo");

let carritoVenta = [];
let productoCargadoActual = null;

const visualizaMarcaProducto = document.getElementById('lblMarcaProducto');
const visualizaNombreProducto = document.getElementById('lblNombreProducto');
const visualizaReferenciaProducto = document.getElementById('lblReferenciaProducto');
const visualizaColorProducto = document.getElementById('lblColorProducto');
const lblCantStockActual = document.getElementById('lblCantStockActual');
let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
const cantUnidadesVender = document.getElementById('validarCantidadVendidos');
const chkManualTotal = document.getElementById('chkManualTotal');
const chkConsumidorFinal = document.getElementById('chkConsumidorFinal');
const inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');

// Referencias a los elementos del formulario
const selectTipoDoc = document.getElementById('TipoDocumentoId');
const txtDocumento = document.getElementById('txtClienteDocumento');
const txtPrimerNombre = document.getElementById('txtPrimerNombre');
const txtSegundoNombre = document.getElementById('txtSegundoNombre');
const txtPrimerApellido = document.getElementById('txtPrimerApellido');
const txtSegundoApellido = document.getElementById('txtSegundoApellido');
const txtTelefono = document.getElementById('txtClienteTelefono');
const txtCorreo = document.getElementById('txtCorreo');
const txtVehiculoPlaca = document.getElementById('txtVehiculoPlaca');
const selectTipoVehiculo = document.getElementById('TipoVehiculoId');
const txtVehiculoModelo = document.getElementById('txtVehiculoModelo');
const txtVehiculoKm = document.getElementById('txtVehiculoKm');
const btnBuscar = document.getElementById('btnBuscarCliente');

let selectTipoDoc_antes = null;
let txtDocumento_antes = null;
let txtPrimerNombre_antes = null;
let txtSegundoNombre_antes = null;
let txtPrimerApellido_antes = null;
let txtSegundoApellido_antes = null;
let txtTelefono_antes = null;
let txtCorreo_antes = null;
let selectTipoVehiculo_antes = null;
let txtVehiculoPlaca_antes = null;
let txtVehiculoModelo_antes = null;
let txtVehiculoKm_antes = null;

/**
 * 1. Carga los lotes desde el Backend filtrando por código de producto
 */
async function CargarLotesProductoVenta(producto) {
    if (!producto || !producto.codigoProducto) {
        console.warn('El producto ingresado no contiene un código válido.');
        return;
    }

    lotesSeleccionados = [];

    try {
        const codigo = encodeURIComponent(producto.codigoProducto);
        const response = await fetch(`/InventarioLotes/MostrarLotesPorProducto?filtroId=${codigo}`);

        mostrarBloqueDatosProducto(true);

        if (response.ok) {
            const lotes = await response.json();

            if (lotes.length > 0) {
                precioVentaBaseOriginal = producto.precioVentaXuni || 0;

                if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

                if (typeof formatoMoneda === 'function') {
                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
                }
            }
            lotesActuales = lotes;
            renderizarLotes(lotes || []);
        } else {
            MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
        }
    } catch (error) {
        console.error('Error en la petición de lotes:', error);
    }
}

/**
 * 2. Renderiza la fila de lotes adaptada a las columnas de BD
 */
function renderizarLotes(lotes) {
    const contenedor = document.getElementById('contenedorLotes');
    if (!contenedor) return;

    contenedor.innerHTML = '';

    if (!lotes || lotes.length === 0) {
        contenedor.innerHTML = '<span class="badge bg-danger">Sin stock de lotes</span>';
        MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
        return;
    }

    lotes.forEach(lote => {
        const idLote = lote.idLote ?? lote.IdLote;
        const stock = lote.cantRestante ?? lote.CantRestante ?? 0;
        const precioCompra = lote.precioCompraXuni ?? lote.PrecioCompraXuni ?? 0;
        const precioVenta = lote.precioVentaXuni ?? lote.PrecioVentaXuni ?? 0;
        const loteIdSanitizado = String(idLote).replace(/[^\w-]/g, '');

        let precioCompraP = SoloFormatoMoneda(precioCompra);
        let precioVentaP = SoloFormatoMoneda(precioVenta);

        const btnLote = document.createElement('button');
        btnLote.type = 'button';
        btnLote.className = 'btn btn-outline-primary btn-sm rounded-pill position-relative me-1 mb-1 btn-lote';
        btnLote.dataset.loteId = loteIdSanitizado;
        btnLote.dataset.stock = stock;
        btnLote.dataset.precio = precioVenta;

        btnLote.innerHTML = `
                Lote <strong>${idLote}</strong> 
                <span class="badge bg-secondary ms-1">${stock} und</span>
                <span class="badge bg-danger ms-1 badge-venta-tooltip"
                      data-bs-toggle="tooltip"
                      data-bs-placement="top"
                      data-bs-title="Venta: ${precioVentaP}" 
                      data-bs-trigger="click"
                      style="cursor: pointer;">
                    <i class="fa fa-low-vision" aria-hidden="true"></i>
                </span>
                <span class="badge bg-success ms-1 badge-compra-tooltip"
                      data-bs-toggle="tooltip"
                      data-bs-placement="top"
                      data-bs-title="Compra: ${precioCompraP}" 
                      data-bs-trigger="click"
                      style="cursor: pointer;">
                    <i class="fa fa-low-vision" aria-hidden="true"></i>
                </span>
            `;

        btnLote.addEventListener('click', () => toggleSeleccionLote(loteIdSanitizado, btnLote));
        contenedor.appendChild(btnLote);

        btnLote.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            let timer;

            el.addEventListener('click', (e) => e.stopPropagation());

            el.addEventListener('shown.bs.tooltip', () => {
                clearTimeout(timer);
                timer = setTimeout(() => tooltip.hide(), 4000);
            });
        });
    });
    actualizarCalculoUnidades();
}

/**
 * 3. Manejo de selección/deselección de lotes manteniendo orden de clic
 * CORREGIDO: Se eliminó la función duplicada
 */
function toggleSeleccionLote(loteId, elementoHtml) {
    const index = lotesSeleccionados.indexOf(loteId);

    if (index > -1) {
        lotesSeleccionados.splice(index, 1);
        elementoHtml.classList.remove('btn-primary', 'active');
        elementoHtml.classList.add('btn-outline-primary');
    } else {
        const cantidadRequerida = parseInt(cantUnidadesVender?.value || 0, 10);

        if (cantidadRequerida <= 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Atención',
                text: 'Ingresa primero la cantidad de unidades a vender.',
                timer: 5000,
                showConfirmButton: false
            });
            return;
        }

        let stockAcumulado = 0;
        document.querySelectorAll('.btn-lote.active').forEach(btn => {
            stockAcumulado += parseInt(btn.dataset.stock || 0, 10);
        });

        if (stockAcumulado >= cantidadRequerida) {
            Swal.fire({
                icon: 'info',
                title: 'Cantidad cubierta',
                text: `La cantidad requerida (${cantidadRequerida} und) ya está totalmente cubierta por los lotes seleccionados.`,
                timer: 5000,
                showConfirmButton: false
            });
            return;
        }

        lotesSeleccionados.push(loteId);
        elementoHtml.classList.remove('btn-outline-primary');
        elementoHtml.classList.add('btn-primary', 'active');
    }

    actualizarCalculoUnidades();
}

/**
 * 4. Recalcula la distribución respetando el orden en que se hizo clic
 */
function calcularDistribucionLotes(cantidadRequerida) {
    if (!Array.isArray(lotesActuales) || lotesActuales.length === 0 || lotesSeleccionados.length === 0) {
        return { distribucion: [], unidadesPendientes: cantidadRequerida };
    }

    let unidadesPendientes = cantidadRequerida;
    const distribucion = [];

    for (const idSeleccionado of lotesSeleccionados) {
        const lote = lotesActuales.find(l => String(l.idLote ?? l.id) === String(idSeleccionado));

        if (!lote) continue;

        const idLote = lote.idLote ?? lote.id;
        const stockDisponible = lote.cantRestante ?? lote.stock ?? 0;

        const tomar = unidadesPendientes > 0 ? Math.min(stockDisponible, unidadesPendientes) : 0;
        unidadesPendientes -= tomar;

        distribucion.push({
            loteId: idLote,
            codigo: idLote,
            tomadas: tomar
        });
    }

    return { distribucion, unidadesPendientes };
}

/**
 * 5. Muestra visualmente la asignación de cada lote
 */
function actualizarCalculoUnidades() {
    const inputCantidad = document.getElementById('validarCantidadVendidos');
    let cantidadAVender = parseInt(inputCantidad?.value, 10);

    if (isNaN(cantidadAVender) || cantidadAVender <= 0) {
        cantidadAVender = 0;
    }

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (!resumen) return;

    if (!lotesActuales || lotesActuales.length === 0) {
        return;
    }

    const stockTotalGlobal = lotesActuales.reduce((suma, lote) => {
        return suma + (lote.cantRestante ?? lote.stock ?? 0);
    }, 0);

    if (cantidadAVender > stockTotalGlobal) {
        cantidadAVender = stockTotalGlobal;
        if (inputCantidad) {
            inputCantidad.value = stockTotalGlobal;
        }
    }

    if (lotesSeleccionados.length === 0) {
        resumen.innerHTML = '<span class="text-warning fs-7">⚠️ Debe seleccionar al menos un lote para procesar la venta.</span>';
        return;
    }

    if (cantidadAVender === 0) {
        resumen.innerHTML = '<span class="text-secondary fs-7">Ingrese una cantidad válida mayor a 0.</span>';
        return;
    }

    const resultado = calcularDistribucionLotes(cantidadAVender);

    let htmlResumen = '<div class="d-flex flex-wrap align-items-center gap-1 mb-1"><span class="fw-bold me-1 text-dark fs-7">Asignación:</span>';

    resultado.distribucion.forEach(item => {
        const badgeColor = item.tomadas > 0 ? 'bg-light text-primary border-primary' : 'bg-light text-muted border-secondary';

        htmlResumen += `
            <span class="badge ${badgeColor} border fs-7 me-1">
                Lote ${item.codigo}: <strong class="text-dark">${item.tomadas} und</strong>
            </span>
        `;
    });

    htmlResumen += '</div>';

    if (resultado.unidadesPendientes > 0) {
        htmlResumen += `<span class="text-danger fw-bold fs-7">⚠️ Faltan ${resultado.unidadesPendientes} unidades por cubrir. Seleccione más lotes.</span>`;
    } else {
        htmlResumen += `<span class="text-success fw-bold fs-7">✔ Cantidad requerida (${cantidadAVender} und) cubierta correctamente.</span>`;
    }

    resumen.innerHTML = htmlResumen;
}

/**
 * 6. Búsqueda y carga del producto
 */
async function CargarProductoExisteParaVenta(limpiarCampos) {
    let codigo = document.getElementById("validarCodigo")?.value;
    let productoEncontrado = false;

    if (limpiarCampos == true) {
        LimpiarCamposVenta();
    }

    if (!codigo) {
        console.log("El campo de código está vacío.");
        return;
    }

    try {
        const response = await fetch(`/Funciones/CargarProductoExisteParaVenta?filtroId=${codigo}`);

        if (response.ok) {
            const producto = await response.json();
            productoCargadoActual = producto;

            if (producto && Object.keys(producto).length > 0) {
                productoEncontrado = true;

                if (lblCantStockActual) {
                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
                }

                if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = producto.nombreMarca || 'N/A';
                if (visualizaColorProducto) visualizaColorProducto.innerText = producto.nombreColor || 'N/A';
                if (visualizaNombreProducto) visualizaNombreProducto.innerText = producto.nombreProducto || 'N/A';
                if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = producto.referencia || 'N/A';

                CargarLotesProductoVenta(producto);
                CargarImagenBase64(previewImagenProducto, producto.imagenProducto);

                if (ImagenProducto) ImagenProducto.value = producto.imagenProducto;
            } else {
                console.log("Producto no encontrado o datos vacíos.");
            }
        } else {
            console.log("No se encontró en la respuesta de la API el producto: ", codigo);
        }
    } catch (error) {
        console.error('Error al buscar producto:', error);
    }

    if (previewImagenProducto) {
        if (productoEncontrado && ImagenProducto?.value) {
            previewImagenProducto.style.display = 'block';
            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'none';
        } else {
            previewImagenProducto.style.display = 'none';
            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';
        }
    }
}

/**
 * 7. Limpieza de interfaz
 */
function LimpiarCamposVenta() {
    lotesSeleccionados = [];
    lotesActuales = null;
    productoCargadoActual = null;

    if (lblCantStockActual) lblCantStockActual.setAttribute("data-valor-interno", '');
    if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = '';
    if (inputPrecioVentaTotal) inputPrecioVentaTotal.value = '';
    if (lblCantStockActual) lblCantStockActual.textContent = '';
    if (cantUnidadesVender) cantUnidadesVender.value = '';
    if (chkManualTotal) chkManualTotal.checked = false;
    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;
    if (precioVentaBaseOriginal) precioVentaBaseOriginal = 0;

    if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = '';
    if (visualizaNombreProducto) visualizaNombreProducto.innerText = '';
    if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = '';
    if (visualizaColorProducto) visualizaColorProducto.innerText = '';

    if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';

    if (previewImagenProducto) {
        previewImagenProducto.style.display = 'none';
        previewImagenProducto.value = null;
    }
    if (ImagenProducto) ImagenProducto.value = null;

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (resumen) resumen.innerHTML = '';

    limpiarLotesCompletamente();    
    mostrarBloqueDatosProducto(false);
}

function mostrarBloqueDatosProducto(mostrar) {
    const contenedor = document.getElementById('contenedorDetallesProducto');
    if (mostrar) {
        if (contenedor) {
            contenedor.classList.remove('d-none');
            contenedor.removeAttribute('aria-hidden');
        }
    } else {
        if (contenedor) {
            contenedor.classList.add('d-none');
            contenedor.setAttribute('aria-hidden', 'true');
        }
    }
}

const limpiarNumero = (val) => parseFloat((val || '').toString().replace(/\D/g, '')) || 0;

if (chkManualTotal) {
    chkManualTotal.addEventListener('change', function () {
        if (this.checked) {
            inputPrecioVentaTotal.removeAttribute('readonly');
            inputPrecioVentaTotal.classList.add('bg-white');
            inputPrecioVentaTotal.focus();

            if (inputPrecioVentaPorUni) {
                inputPrecioVentaPorUni.readOnly = true;
                inputPrecioVentaPorUni.classList.add('bg-light');
            }
            calcularUnidadDesdeTotal();
        } else {
            inputPrecioVentaTotal.readOnly = true;
            inputPrecioVentaTotal.classList.remove('bg-white');

            if (inputPrecioVentaPorUni) {
                inputPrecioVentaPorUni.readOnly = false;
                inputPrecioVentaPorUni.classList.remove('bg-light');

                inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

                if (typeof formatoMoneda === 'function') {
                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
                }

                calcularVenta();
            }

            if (typeof actualizarCalculoUnidades === 'function') {
                actualizarCalculoUnidades();
            }
        }
    });
}

if (chkConsumidorFinal) {
    chkConsumidorFinal.addEventListener('change', function () {
        const esConsumidorFinal = this.checked;

        const inputsCliente = [
            txtDocumento,
            txtPrimerNombre,
            txtSegundoNombre,
            txtPrimerApellido,
            txtSegundoApellido,
            txtTelefono,
            txtCorreo,
            selectTipoVehiculo,
            txtVehiculoPlaca,
            txtVehiculoModelo,
            txtVehiculoKm
        ];

        if (esConsumidorFinal) {
            selectTipoDoc_antes = selectTipoDoc?.value;
            txtDocumento_antes = txtDocumento?.value;
            txtPrimerNombre_antes = txtPrimerNombre?.value;
            txtSegundoNombre_antes = txtSegundoNombre?.value;
            txtPrimerApellido_antes = txtPrimerApellido?.value;
            txtSegundoApellido_antes = txtSegundoApellido?.value;
            txtTelefono_antes = txtTelefono?.value;
            txtCorreo_antes = txtCorreo?.value;
            txtVehiculoPlaca_antes = txtVehiculoPlaca?.value;
            selectTipoVehiculo_antes = selectTipoVehiculo?.value;
            txtVehiculoModelo_antes = txtVehiculoModelo?.value;
            txtVehiculoKm_antes = txtVehiculoKm?.value;

            if (selectTipoDoc) selectTipoDoc.value = '1';
            if (txtDocumento) txtDocumento.value = '222222222222';
            if (txtPrimerNombre) txtPrimerNombre.value = 'Clientes';
            if (txtSegundoNombre) txtSegundoNombre.value = '';
            if (txtPrimerApellido) txtPrimerApellido.value = 'Varios';
            if (txtSegundoApellido) txtSegundoApellido.value = '';
            if (txtTelefono) txtTelefono.value = '0000000000';
            if (txtCorreo) txtCorreo.value = '';
            if (selectTipoVehiculo) selectTipoVehiculo.value = '';
            if (txtVehiculoPlaca) txtVehiculoPlaca.value = '';
            if (txtVehiculoModelo) txtVehiculoModelo.value = '';
            if (txtVehiculoKm) txtVehiculoKm.value = '';

            inputsCliente.forEach(input => {
                if (input) input.readOnly = true;
            });

            if (selectTipoDoc) {
                selectTipoDoc.style.pointerEvents = 'none';
                selectTipoDoc.style.backgroundColor = '#e9ecef';
                selectTipoDoc.tabIndex = -1;
            }

            if (selectTipoVehiculo) {
                selectTipoVehiculo.style.pointerEvents = 'none';
                selectTipoVehiculo.style.backgroundColor = '#e9ecef';
                selectTipoVehiculo.tabIndex = -1;
            }

            if (btnBuscar) btnBuscar.disabled = true;

        } else {
            LimpiarCamposCliente();
            if (selectTipoDoc) selectTipoDoc.value = selectTipoDoc_antes;
            if (txtDocumento) txtDocumento.value = txtDocumento_antes;
            if (txtPrimerNombre) txtPrimerNombre.value = txtPrimerNombre_antes;
            if (txtSegundoNombre) txtSegundoNombre.value = txtSegundoNombre_antes;
            if (txtPrimerApellido) txtPrimerApellido.value = txtPrimerApellido_antes;
            if (txtSegundoApellido) txtSegundoApellido.value = txtSegundoApellido_antes;
            if (txtTelefono) txtTelefono.value = txtTelefono_antes;
            if (txtCorreo) txtCorreo.value = txtCorreo_antes;
            if (txtVehiculoPlaca) txtVehiculoPlaca.value = txtVehiculoPlaca_antes;
            if (selectTipoVehiculo) selectTipoVehiculo.value = selectTipoVehiculo_antes;
            if (txtVehiculoModelo) txtVehiculoModelo.value = txtVehiculoModelo_antes;
            if (txtVehiculoKm) txtVehiculoKm.value = txtVehiculoKm_antes;
        }
    });
}

function LimpiarCamposCliente() {
    const idsInputs = [
        'txtClienteDocumento',
        'txtPrimerNombre',
        'txtSegundoNombre',
        'txtPrimerApellido',
        'txtSegundoApellido',
        'txtClienteTelefono',
        'txtCorreo',
        'txtVehiculoPlaca',
        'txtVehiculoModelo',
        'txtVehiculoKm'
    ];

    // CORREGIDO: Se usa la variable global chkConsumidorFinal
    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;

    idsInputs.forEach(id => {
        const input = document.getElementById(id);
        if (input) {
            input.value = '';
            input.readOnly = false;
        }
    });

    if (selectTipoDoc) {
        selectTipoDoc.value = '';
        selectTipoDoc.style.pointerEvents = '';
        selectTipoDoc.style.backgroundColor = '';
        selectTipoDoc.removeAttribute('tabindex');
    }

    if (selectTipoVehiculo) {
        selectTipoVehiculo.value = '';
        selectTipoVehiculo.style.pointerEvents = '';
        selectTipoVehiculo.style.backgroundColor = '';
        selectTipoVehiculo.removeAttribute('tabindex');
    }

    if (btnBuscar) btnBuscar.disabled = false;
}

function calcularUnidadDesdeTotal() {
    const total = limpiarNumero(inputPrecioVentaTotal.value);
    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;

    if (cantidad > 0) {
        const precioUnidad = total / cantidad;
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precioUnidad);
    } else {
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(0);
    }

    inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
}

if (inputPrecioVentaTotal) {
    inputPrecioVentaTotal.addEventListener('input', function () {
        if (chkManualTotal?.checked) {
            calcularUnidadDesdeTotal();
        }
    });

    inputPrecioVentaTotal.addEventListener('blur', function () {
        if (chkManualTotal?.checked) {
            const num = limpiarNumero(this.value);
            this.value = SoloFormatoMoneda(num);
        }
    });
}

if (cantUnidadesVender) {
    cantUnidadesVender.addEventListener('input', function () {
        if (chkManualTotal?.checked) {
            calcularUnidadDesdeTotal();
        } else if (typeof actualizarCalculoUnidades === 'function') {
            actualizarCalculoUnidades();
        }
    });
}

function calcularVenta() {
    const stockActual = parseInt(lblCantStockActual?.getAttribute("data-valor-interno") || 0);
    const cantUniVender = parseFloat(cantUnidadesVender?.value) || 0;
    const precio = parseCurrency(inputPrecioVentaPorUni?.value);

    let total = 0;

    if (cantUniVender > stockActual) {
        total = stockActual * precio;
    } else {
        total = cantUniVender * precio;
    }

    if (inputPrecioVentaPorUni) {
        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precio);
    }

    if (inputPrecioVentaTotal) {
        inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
    }
}

function limpiarLotesCompletamente() {
    lotesSeleccionados = [];

    const contenedorLotes = document.getElementById('contenedorLotes');

    if (contenedorLotes) {
        contenedorLotes.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            const instance = bootstrap.Tooltip.getInstance(el);
            if (instance) instance.dispose();
        });

        contenedorLotes.innerHTML = '';
    }

    const contenedorAsignacion = document.getElementById('contenedorAsignacion');
    if (contenedorAsignacion) {
        contenedorAsignacion.innerHTML = '';
    }

    if (typeof actualizarCalculoUnidades === 'function') {
        actualizarCalculoUnidades();
    }
}

function agregarProductoACarrito(productoActual) {
    const yaExiste = carritoVenta.some(item => String(item.codigoProducto) === String(productoActual.codigoProducto));

    if (yaExiste) {
        MostrarAlerta("warning", "Atención", "Este producto ya fue agregado a la lista de venta.", 4000);
        return;
    }

    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;
    const precioUnid = limpiarNumero(inputPrecioVentaPorUni.value);
    const total = cantidad * precioUnid;

    if (cantidad <= 0 || precioUnid <= 0) {
        MostrarAlerta("warning", "Atención", "Ingrese una cantidad y precio válidos.", 4000);
        return;
    }

    if (!lotesSeleccionados || lotesSeleccionados.length === 0) {
        MostrarAlerta("warning", "Atención", "Debe seleccionar al menos un lote para este producto.", 4000);
        return;
    }

    const distribucionResult = calcularDistribucionLotes(cantidad);

    if (distribucionResult.unidadesPendientes > 0) {
        MostrarAlerta("warning", "Atención", `Aún faltan ${distribucionResult.unidadesPendientes} unidades por cubrir. Seleccione más lotes.`, 5000);
        return;
    }

    const lotesEstandarizados = distribucionResult.distribucion
        .filter(item => item.tomadas > 0)
        .map(item => {
            const loteObj = lotesActuales.find(l => String(l.idLote ?? l.IdLote ?? l.id) === String(item.loteId));

            const id = item.loteId;

            let refLote = loteObj?.referencia || loteObj?.Referencia || loteObj?.nombreLote || loteObj?.codigoLote || loteObj?.numLote;

            if (!refLote || refLote.toString().trim().toUpperCase() === 'NO APLICA' || refLote.toString().trim() === '') {
                refLote = `Lote ${id}`;
            }

            return {
                idLote: id,
                referencia: refLote,
                cantidad: item.tomadas
            };
        });

    let nomProducto = (productoActual.nombreProducto || '').toUpperCase().trim();
    let marcaProducto = (productoActual.nombreMarca || '').toUpperCase().trim();
    let colorProducto = (productoActual.nombreColor || '').toUpperCase().trim();
    let referenciaProducto = (productoActual.referencia || '').toUpperCase().trim();

    let detalleProducto = [nomProducto, marcaProducto, colorProducto, referenciaProducto]
        .filter(val => val !== '' && val !== 'NO APLICA')
        .join(' - ');

    const detalleItem = {
        codigoProducto: productoActual.codigoProducto,
        nombreProducto: detalleProducto,
        cantidad: cantidad,
        precioUnitario: precioUnid,
        total: total,
        lotes: lotesEstandarizados
    };

    carritoVenta.push(detalleItem);
    renderizarTablaVenta();
    LimpiarCamposVenta();
    if (validarCodigo) validarCodigo.value = '';
}

function renderizarTablaVenta() {
    const tbody = document.getElementById('tbodyDetalleVenta');
    const lblTotalPagar = document.getElementById('lblTotalPagar');

    if (!tbody) return;

    if (carritoVenta.length === 0) {
        tbody.innerHTML = `
            <tr id="trFilaVacia">
                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
            </tr>`;
        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
        return;
    }

    let html = '';
    let granTotal = 0;

    carritoVenta.forEach((item, index) => {
        granTotal += item.total;

        const textoLotes = (item.lotes && item.lotes.length > 0)
            ? item.lotes.map(l => `${l.referencia} (${l.cantidad} und)`).join(', ')
            : 'Sin lote asignado';

        html += `
            <tr>
                <td class="text-center fw-bold fs-7">${item.codigoProducto}</td>
                <td>
                    <div class="fw-bold">${item.nombreProducto}</div>
                    <small class="text-muted fs-8 d-block">
                        Lotes: ${textoLotes}
                    </small>
                </td>
                <td class="text-center fw-bold">${item.cantidad}</td>
                <td class="text-end">${SoloFormatoMoneda(item.precioUnitario)}</td>
                <td class="text-end fw-bold text-success">${SoloFormatoMoneda(item.total)}</td>
                <td class="text-center">
                    <button class="btn btn-outline-danger btn-sm border-0" onclick="eliminarItemCarrito(${index})">
                        <i class="fa-solid fa-trash-arrow-up"></i> Quitar
                    </button>
                </td>
            </tr>`;
    });

    tbody.innerHTML = html;
    if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(granTotal);
}

function eliminarItemCarrito(index) {
    carritoVenta.splice(index, 1);
    renderizarTablaVenta();
    if (typeof calcularTotalPagado_Y_Restante === 'function') {
        calcularTotalPagado_Y_Restante(null, null, null);
    }
}

async function registrarVenta() {
    if (carritoVenta.length === 0) {
        MostrarAlerta("warning", "Venta Vacía", "Agregue al menos un producto a la tabla.", 4000);
        return;
    }

    const payloadVenta = {
        totalVenta: carritoVenta.reduce((acc, item) => acc + item.total, 0),
        detalles: carritoVenta
    };

    try {
        const response = await fetch('/Ventas/RegistrarVenta', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payloadVenta)
        });

        if (response.ok) {
            MostrarAlerta("success", "Éxito", "La venta y el descuento de lotes se registraron correctamente.", 5000);
            carritoVenta = [];
            renderizarTablaVenta();
        } else {
            const errorMsg = await response.text();
            MostrarAlerta("error", "Error en Venta", errorMsg || "No se pudo registrar la venta.", 5000);
        }
    } catch (error) {
        console.error('Error enviando la venta:', error);
        MostrarAlerta("error", "Conexión", "Error de red al intentar procesar la venta.", 5000);
    }
}

function alCargarProductoExitoso(producto) {
    productoCargadoActual = producto;
    if (visualizaMarcaProducto) visualizaMarcaProducto.textContent = producto.marca;
    if (visualizaNombreProducto) visualizaNombreProducto.textContent = producto.nombre;
}

function prepararYAgregarProducto() {
    if (!productoCargadoActual) {
        MostrarAlerta("warning", "Atención", "Primero debe buscar y seleccionar un producto.", 4000);
        return;
    }

    agregarProductoACarrito(productoCargadoActual);
    if (typeof calcularTotalPagado_Y_Restante === 'function') {
        calcularTotalPagado_Y_Restante(null, null, null);
    }
}

/**
 * CORREGIDO: Se agregó 'async' a la declaración de la función
 */
async function BuscarCliente() {
    let clienteEncontrado = false;

    // 1. Aseguramos conversión a String y eliminamos espacios en blanco
    const numeroDocumento = String(txtDocumento?.value || '').trim();

    // 2. Validamos la longitud real del texto
    if (!numeroDocumento || numeroDocumento.length < 6) {
        MostrarAlerta("warning", "Documento incompleto", "Agregue un número de documento válido.", 5000);
        return;
    }

    try {
        const response = await fetch(`/Usuarios/ObtenerUsuario?filtroId=${encodeURIComponent(numeroDocumento)}`);

        if (response.ok) {
            const clienteActual = await response.json();

            // 3. Manejo seguro para determinar si viene un Objeto o un Array con datos
            const esValido = clienteActual && (
                Array.isArray(clienteActual)
                    ? clienteActual.length > 0
                    : Object.keys(clienteActual).length > 0
            );

            if (esValido) {
                // Si la respuesta es un Array, se toma el primer registro
                const cliente = Array.isArray(clienteActual) ? clienteActual[0] : clienteActual;

                clienteEncontrado = true;

                // Mapeo con soporte para camelCase y PascalCase
                if (selectTipoDoc) selectTipoDoc.value = cliente.tipoDocumentos.tipoDocumentoId ?? cliente.tipoDocumentos.tipoDocumentoId ?? '';
                if (txtDocumento) txtDocumento.value = cliente.documento ?? cliente.documento ?? '';
                if (txtPrimerNombre) txtPrimerNombre.value = cliente.primerNombre ?? cliente.primerNombre ?? '';
                if (txtSegundoNombre) txtSegundoNombre.value = cliente.segundoNombre ?? cliente.segundoNombre ?? '';
                if (txtPrimerApellido) txtPrimerApellido.value = cliente.primerApellido ?? cliente.primerApellido ?? '';
                if (txtSegundoApellido) txtSegundoApellido.value = cliente.segundoApellido ?? cliente.segundoApellido ?? '';
                if (txtTelefono) txtTelefono.value = cliente.telefonoMovil ?? cliente.telefonoMovil ?? '';
                if (txtCorreo) txtCorreo.value = cliente.correo ?? cliente.correo ?? '';
            } else {
                MostrarAlerta("warning", "Cliente", "Datos vacíos del usuario.", 5000);
            }
        } else {
            MostrarAlerta("warning", "Cliente", "Usuario no encontrado.", 5000);
        }
    } catch (error) {
        console.error('Error al buscar cliente:', error);
        MostrarAlerta("warning", "Cliente", "Error al buscar usuario: " + error, 5000);
    }
}



















//// Variable global o de ámbito de la venta
//let lotesActuales = null;
//let lotesSeleccionados = []; // Guarda los IDs de los lotes activos en orden de clic
//let precioVentaBaseOriginal = 0;
//let ImagenProducto = document.getElementById('ImagenProducto');
//let previewImagenProducto = document.getElementById('previewImagenProducto');
//let carritoVenta = [];
//let productoCargadoActual = null;
////let validarCodigo = document.getElementById("validarCodigo");

//const visualizaMarcaProducto = document.getElementById('lblMarcaProducto');
//const visualizaNombreProducto = document.getElementById('lblNombreProducto');
//const visualizaReferenciaProducto = document.getElementById('lblReferenciaProducto');
//const visualizaColorProducto = document.getElementById('lblColorProducto');
//const lblCantStockActual = document.getElementById('lblCantStockActual');
//let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
//const cantUnidadesVender = document.getElementById('validarCantidadVendidos');
//const chkManualTotal = document.getElementById('chkManualTotal');
//const chkConsumidorFinal = document.getElementById('chkConsumidorFinal');
//const inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');

//// Referencias a los elementos del formulario
//const selectTipoDoc = document.getElementById('TipoDocumentoId');
//const txtDocumento = document.getElementById('txtClienteDocumento');
//const txtPrimerNombre = document.getElementById('txtPrimerNombre');
//const txtSegundoNombre = document.getElementById('txtSegundoNombre');
//const txtPrimerApellido = document.getElementById('txtPrimerApellido');
//const txtSegundoApellido = document.getElementById('txtSegundoApellido');
//const txtTelefono = document.getElementById('txtClienteTelefono');
//const txtCorreo = document.getElementById('txtCorreo');
//const txtVehiculoPlaca = document.getElementById('txtVehiculoPlaca');
//const selectTipoVehiculo = document.getElementById('TipoVehiculoId');
//const txtVehiculoModelo = document.getElementById('txtVehiculoModelo');
//const txtVehiculoKm = document.getElementById('txtVehiculoKm');
//const btnBuscar = document.getElementById('btnBuscarCliente');


//let selectTipoDoc_antes = null;
//let txtDocumento_antes = null;
//let txtPrimerNombre_antes = null;
//let txtSegundoNombre_antes = null;
//let txtPrimerApellido_antes = null;
//let txtSegundoApellido_antes = null;
//let txtTelefono_antes = null;
//let txtCorreo_antes = null;
//let selectTipoVehiculo_antes = null;
//let txtVehiculoPlaca_antes = null;
//let txtVehiculoModelo_antes = null;
//let txtVehiculoKm_antes = null;

///**
// * 1. Carga los lotes desde el Backend filtrando por código de producto
// */
//async function CargarLotesProductoVenta(producto) {
//    if (!producto || !producto.codigoProducto) {
//        console.warn('El producto ingresado no contiene un código válido.');
//        return;
//    }

//    // CORREGIDO: Limpiar arreglo en lugar de .clear()
//    lotesSeleccionados = [];

//    try {
//        const codigo = encodeURIComponent(producto.codigoProducto);
//        const response = await fetch(`/InventarioLotes/MostrarLotesPorProducto?filtroId=${codigo}`);

//        //const contenedor = document.getElementById('contenedorDetallesProducto');
//        //if (contenedor) {
//        //    contenedor.classList.remove('d-none');
//        //    contenedor.removeAttribute('aria-hidden');
//        //}

//        mostrarBloqueDatosProducto(true);

//        if (response.ok) {
//            const lotes = await response.json();

//            if (lotes.length > 0) {
//                // 1. Guardamos el precio base original en la variable global
//                precioVentaBaseOriginal = producto.precioVentaXuni || 0;

//                if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

//                if (typeof formatoMoneda === 'function') {
//                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
//                }
//            }
//            lotesActuales = lotes;
//            renderizarLotes(lotes || []);
//        } else {
//            MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
//        }
//    } catch (error) {
//        console.error('Error en la petición de lotes:', error);
//    }
//}

///**
// * 2. Renderiza la fila de lotes adaptada a las columnas de BD
// */
//function renderizarLotes(lotes) {
//    const contenedor = document.getElementById('contenedorLotes');
//    if (!contenedor) return;

//    contenedor.innerHTML = '';

//    if (!lotes || lotes.length === 0) {
//        contenedor.innerHTML = '<span class="badge bg-danger">Sin stock de lotes</span>';
//        MostrarAlerta("info", "Stock - Lotes", "El producto No tiene lotes registrados.", 7000, false, null);
//        return;
//    }

//    lotes.forEach(lote => {
//        const idLote = lote.idLote ?? lote.IdLote;
//        const stock = lote.cantRestante ?? lote.CantRestante ?? 0;
//        const precioCompra = lote.precioCompraXuni ?? lote.PrecioCompraXuni ?? 0;
//        const precioVenta = lote.precioVentaXuni ?? lote.PrecioVentaXuni ?? 0;
//        const loteIdSanitizado = String(idLote).replace(/[^\w-]/g, '');

//        let precioCompraP = SoloFormatoMoneda(precioCompra);
//        let precioVentaP = SoloFormatoMoneda(precioVenta);

//        const btnLote = document.createElement('button');
//        btnLote.type = 'button';
//        btnLote.className = 'btn btn-outline-primary btn-sm rounded-pill position-relative me-1 mb-1 btn-lote';
//        btnLote.dataset.loteId = loteIdSanitizado;
//        btnLote.dataset.stock = stock;
//        btnLote.dataset.precio = precioVenta;

//        // Le agregamos una clase identificadora (.badge-compra-tooltip) al span de compra
//        btnLote.innerHTML = `
//                Lote <strong>${idLote}</strong> 
//                <span class="badge bg-secondary ms-1">${stock} und</span>
//                <span class="badge bg-danger ms-1 badge-venta-tooltip"
//                      data-bs-toggle="tooltip"
//                      data-bs-placement="top"
//                      data-bs-title="Venta: ${precioVentaP}" 
//                      data-bs-trigger="click"
//                      style="cursor: pointer;">
//                    <i class="fa fa-low-vision" aria-hidden="true"></i>
//                </span>
//                <span class="badge bg-success ms-1 badge-compra-tooltip"
//                      data-bs-toggle="tooltip"
//                      data-bs-placement="top"
//                      data-bs-title="Compra: ${precioCompraP}" 
//                      data-bs-trigger="click"
//                      style="cursor: pointer;">
//                    <i class="fa fa-low-vision" aria-hidden="true"></i>
//                </span>
//            `;

//        // Evento para seleccionar el lote
//        btnLote.addEventListener('click', () => toggleSeleccionLote(loteIdSanitizado, btnLote));

//        // Insertamos el botón en el contenedor
//        contenedor.appendChild(btnLote);

//        // --- 💡 INICIALIZACIÓN Y CONTROL DEL TOOLTIP ---
//        btnLote.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
//            const tooltip = new bootstrap.Tooltip(el);
//            let timer;

//            // 1. Evita que el clic seleccione el lote
//            el.addEventListener('click', (e) => e.stopPropagation());

//            // 2. Oculta el tooltip automáticamente a los 4 segundos
//            el.addEventListener('shown.bs.tooltip', () => {
//                clearTimeout(timer);
//                timer = setTimeout(() => tooltip.hide(), 4000);
//            });
//        });
//    });
//    actualizarCalculoUnidades();
//}

///**
// * 3. Manejo de selección/deselección de lotes manteniendo orden de clic
// */
//function toggleSeleccionLote(loteId, elementoHtml) {
//    const index = lotesSeleccionados.indexOf(loteId);

//    if (index > -1) {
//        lotesSeleccionados.splice(index, 1);
//        elementoHtml.classList.remove('btn-primary', 'active');
//        elementoHtml.classList.add('btn-outline-primary');
//    } else {
//        lotesSeleccionados.push(loteId);
//        elementoHtml.classList.remove('btn-outline-primary');
//        elementoHtml.classList.add('btn-primary', 'active');
//    }

//    actualizarCalculoUnidades();
//}

//function toggleSeleccionLote(loteId, elementoHtml) {
//    const index = lotesSeleccionados.indexOf(loteId);

//    if (index > -1) {
//        // DESELECCIONAR: Siempre se permite quitar un lote
//        lotesSeleccionados.splice(index, 1);
//        elementoHtml.classList.remove('btn-primary', 'active');
//        elementoHtml.classList.add('btn-outline-primary');
//    } else {
//        // SELECCIONAR: Validamos antes de permitir agregar un nuevo lote
//        const cantidadRequerida = parseInt(cantUnidadesVender?.value || 0, 10);

//        if (cantidadRequerida <= 0) {
//            Swal.fire({
//                icon: 'warning',
//                title: 'Atención',
//                text: 'Ingresa primero la cantidad de unidades a vender.',
//                timer: 5000,
//                showConfirmButton: false
//            });
//            return;
//        }

//        // Sumamos el stock de los botones de lotes que ya están seleccionados (.active)
//        let stockAcumulado = 0;
//        document.querySelectorAll('.btn-lote.active').forEach(btn => {
//            stockAcumulado += parseInt(btn.dataset.stock || 0, 10);
//        });

//        // Si el stock ya es igual o mayor a lo requerido, bloqueamos la selección
//        if (stockAcumulado >= cantidadRequerida) {
//            Swal.fire({
//                icon: 'info',
//                title: 'Cantidad cubierta',
//                text: `La cantidad requerida (${cantidadRequerida} und) ya está totalmente cubierta por los lotes seleccionados.`,
//                timer: 5000,
//                showConfirmButton: false
//            });
//            return; // Cancela la selección
//        }

//        // Si pasa la validación, se agrega a la lista y se marca activo
//        lotesSeleccionados.push(loteId);
//        elementoHtml.classList.remove('btn-outline-primary');
//        elementoHtml.classList.add('btn-primary', 'active');
//    }

//    actualizarCalculoUnidades();
//}


///**
// * 4. Recalcula la distribución respetando el orden en que se hizo clic
// */
//function calcularDistribucionLotes(cantidadRequerida) {
//    if (!Array.isArray(lotesActuales) || lotesActuales.length === 0 || lotesSeleccionados.length === 0) {
//        return { distribucion: [], unidadesPendientes: cantidadRequerida };
//    }

//    let unidadesPendientes = cantidadRequerida;
//    const distribucion = [];

//    for (const idSeleccionado of lotesSeleccionados) {
//        const lote = lotesActuales.find(l => String(l.idLote ?? l.id) === String(idSeleccionado));

//        if (!lote) continue;

//        const idLote = lote.idLote ?? lote.id;
//        const stockDisponible = lote.cantRestante ?? lote.stock ?? 0;

//        const tomar = unidadesPendientes > 0 ? Math.min(stockDisponible, unidadesPendientes) : 0;
//        unidadesPendientes -= tomar;

//        distribucion.push({
//            loteId: idLote,
//            codigo: idLote,
//            tomadas: tomar
//        });
//    }

//    return { distribucion, unidadesPendientes };
//}

///**
// * 5. Muestra visualmente la asignación de cada lote
// */
//function actualizarCalculoUnidades() {
//    const inputCantidad = document.getElementById('validarCantidadVendidos');
//    let cantidadAVender = parseInt(inputCantidad?.value, 10);

//    // 1. Validar que sea un número válido y mayor a 0
//    if (isNaN(cantidadAVender) || cantidadAVender <= 0) {
//        cantidadAVender = 0;
//    }

//    const resumen = document.getElementById('resumenSeleccionLotes');
//    if (!resumen) return;

//    if (!lotesActuales || lotesActuales.length === 0) {
//        //resumen.innerHTML = '<span class="text-warning fs-7">Sin stock.</span>';
//        return;
//    }

//    // 2. Calcular el STOCK TOTAL GLOBAL de todos los lotes
//    const stockTotalGlobal = lotesActuales.reduce((suma, lote) => {
//        return suma + (lote.cantRestante ?? lote.stock ?? 0);
//    }, 0);

//    // 3. CONTROL DE TOPE MÁXIMO: Si pide más del stock total, se corrige automáticamente
//    if (cantidadAVender > stockTotalGlobal) {
//        cantidadAVender = stockTotalGlobal;
//        if (inputCantidad) {
//            inputCantidad.value = stockTotalGlobal; // Actualiza visualmente el input a la cantidad real permitida
//        }
//    }

//    if (lotesSeleccionados.length === 0) {
//        resumen.innerHTML = '<span class="text-warning fs-7">⚠️ Debe seleccionar al menos un lote para procesar la venta.</span>';
//        return;
//    }

//    if (cantidadAVender === 0) {
//        resumen.innerHTML = '<span class="text-secondary fs-7">Ingrese una cantidad válida mayor a 0.</span>';
//        return;
//    }

//    // 4. Ejecutar algoritmo de distribución con la cantidad YA CORREGIDA
//    const resultado = calcularDistribucionLotes(cantidadAVender);

//    let htmlResumen = '<div class="d-flex flex-wrap align-items-center gap-1 mb-1"><span class="fw-bold me-1 text-dark fs-7">Asignación:</span>';

//    resultado.distribucion.forEach(item => {
//        const badgeColor = item.tomadas > 0 ? 'bg-light text-primary border-primary' : 'bg-light text-muted border-secondary';

//        htmlResumen += `
//            <span class="badge ${badgeColor} border fs-7 me-1">
//                Lote ${item.codigo}: <strong class="text-dark">${item.tomadas} und</strong>
//            </span>
//        `;
//    });

//    htmlResumen += '</div>';

//    // 5. Mensajes de estado
//    if (resultado.unidadesPendientes > 0) {
//        htmlResumen += `<span class="text-danger fw-bold fs-7">⚠️ Faltan ${resultado.unidadesPendientes} unidades por cubrir. Seleccione más lotes.</span>`;
//    } else {
//        htmlResumen += `<span class="text-success fw-bold fs-7">✔ Cantidad requerida (${cantidadAVender} und) cubierta correctamente.</span>`;
//    }

//    resumen.innerHTML = htmlResumen;
//}


///**
// * 6. Búsqueda y carga del producto
// */
//async function CargarProductoExisteParaVenta(limpiarCampos) {
//    let codigo = document.getElementById("validarCodigo")?.value;
//    let productoEncontrado = false;

//    if (limpiarCampos == true) {
//        LimpiarCamposVenta();
//    }

//    if (!codigo) {
//        console.log("El campo de código está vacío.");
//        return;
//    }

//    try {
//        const response = await fetch(`/Funciones/CargarProductoExisteParaVenta?filtroId=${codigo}`);

//        if (response.ok) {
//            const producto = await response.json();
//            productoCargadoActual = producto;

//            if (producto && Object.keys(producto).length > 0) {
//                productoEncontrado = true;

//                if (lblCantStockActual) {
//                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
//                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
//                }

//                if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = producto.nombreMarca || 'N/A';
//                if (visualizaColorProducto) visualizaColorProducto.innerText = producto.nombreColor || 'N/A';
//                if (visualizaNombreProducto) visualizaNombreProducto.innerText = producto.nombreProducto || 'N/A';
//                if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = producto.referencia || 'N/A';

//                CargarLotesProductoVenta(producto);
//                CargarImagenBase64(previewImagenProducto, producto.imagenProducto);

//                if (ImagenProducto) ImagenProducto.value = producto.imagenProducto;
//            } else {
//                console.log("Producto no encontrado o datos vacíos.");
//            }
//        } else {
//            console.log("No se encontró en la respuesta de la API el producto: ", codigo);
//        }
//    } catch (error) {
//        console.error('Error al buscar producto:', error);
//    }

//    const iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
//    if (previewImagenProducto) {
//        if (productoEncontrado && ImagenProducto?.value) {
//            previewImagenProducto.style.display = 'block';
//            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'none';
//        } else {
//            previewImagenProducto.style.display = 'none';
//            if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';
//        }
//    }
//}

///**
// * 7. Limpieza de interfaz
// */
//function LimpiarCamposVenta() {
//    lotesSeleccionados = [];
//    lotesActuales = null;
//    productoCargadoActual = null;

//    if (lblCantStockActual) lblCantStockActual.setAttribute("data-valor-interno", '');
//    if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = '';
//    if (inputPrecioVentaTotal) inputPrecioVentaTotal.value = '';
//    if (lblCantStockActual) lblCantStockActual.textContent = '';
//    if (cantUnidadesVender) cantUnidadesVender.value = '';
//    if (chkManualTotal) chkManualTotal.checked = false;
//    if (chkConsumidorFinal) chkConsumidorFinal.checked = false;
//    if (precioVentaBaseOriginal) precioVentaBaseOriginal = 0;

//    if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = '';
//    if (visualizaNombreProducto) visualizaNombreProducto.innerText = '';
//    if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = '';
//    if (visualizaColorProducto) visualizaColorProducto.innerText = '';

//    if (iconoPreviewImagenProducto) iconoPreviewImagenProducto.style.display = 'block';

//    if (previewImagenProducto) {
//        previewImagenProducto.style.display = 'none';
//        previewImagenProducto.value = null;
//    }
//    if (ImagenProducto) ImagenProducto.value = null;

//    const resumen = document.getElementById('resumenSeleccionLotes');
//    if (resumen) resumen.innerHTML = '';

//    limpiarLotesCompletamente();
//    LimpiarCamposCliente();
//    mostrarBloqueDatosProducto(false);
//}


//function mostrarBloqueDatosProducto(mostrar) {
//    const contenedor = document.getElementById('contenedorDetallesProducto');
//    if (mostrar) {
//        if (contenedor) {
//            contenedor.classList.remove('d-none');
//            contenedor.removeAttribute('aria-hidden');
//        }
//    } else {
//        contenedor.classList.add('d-none');
//        contenedor.setAttribute('aria-hidden', 'true');
//    }
//}


//// Limpia un texto formateado ($ 75.000) a un número puro (75000)
//const limpiarNumero = (val) => parseFloat((val || '').toString().replace(/\D/g, '')) || 0;


//chkManualTotal.addEventListener('change', function () {
//    if (this.checked) {
//        // Modo manual activo...
//        inputPrecioVentaTotal.removeAttribute('readonly');
//        inputPrecioVentaTotal.classList.add('bg-white');
//        inputPrecioVentaTotal.focus();

//        if (inputPrecioVentaPorUni) {
//            inputPrecioVentaPorUni.readOnly = true;
//            inputPrecioVentaPorUni.classList.add('bg-light');
//        }
//        calcularUnidadDesdeTotal();
//    } else {
//        // MODO AUTOMÁTICO: Restauramos el valor original guardado
//        inputPrecioVentaTotal.readOnly = true;
//        inputPrecioVentaTotal.classList.remove('bg-white');

//        if (inputPrecioVentaPorUni) {
//            inputPrecioVentaPorUni.readOnly = false;
//            inputPrecioVentaPorUni.classList.remove('bg-light');

//            // Reasignamos el precio base original
//            inputPrecioVentaPorUni.value = precioVentaBaseOriginal;

//            if (typeof formatoMoneda === 'function') {
//                formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
//            }

//            calcularVenta();
//        }

//        // Recalculamos totales con el valor restaurado
//        if (typeof actualizarCalculoUnidades === 'function') {
//            actualizarCalculoUnidades();
//        }
//    }
//});


//chkConsumidorFinal.addEventListener('change', function () {
//    const esConsumidorFinal = this.checked;
        
//    // Arreglo con los inputs de texto para procesarlos juntos
//    const inputsCliente = [
//        txtDocumento,
//        txtPrimerNombre,
//        txtSegundoNombre,
//        txtPrimerApellido,
//        txtSegundoApellido,
//        txtTelefono,
//        txtCorreo,
//        selectTipoVehiculo,
//        txtVehiculoPlaca,
//        txtVehiculoModelo,
//        txtVehiculoKm
//    ];

//    if (esConsumidorFinal) {
//        selectTipoDoc_antes = selectTipoDoc.value;
//        txtDocumento_antes = txtDocumento.value;
//        txtPrimerNombre_antes = txtPrimerNombre.value;
//        txtSegundoNombre_antes = txtSegundoNombre.value;
//        txtPrimerApellido_antes = txtPrimerApellido.value;
//        txtSegundoApellido_antes = txtSegundoApellido.value;
//        txtTelefono_antes = txtTelefono.value;
//        txtCorreo_antes = txtCorreo.value;
//        txtVehiculoPlaca_antes = txtVehiculoPlaca.value;
//        selectTipoVehiculo_antes = selectTipoVehiculo.value;
//        txtVehiculoModelo_antes = txtVehiculoModelo.value;
//        txtVehiculoKm_antes = txtVehiculoKm.value;


//        // 1. Asignar valores por defecto
//        selectTipoDoc.value = '1'; // Cédula (ID 1)
//        txtDocumento.value = '222222222222';
//        txtPrimerNombre.value = 'Clientes';
//        txtSegundoNombre.value = '';
//        txtPrimerApellido.value = 'Varios';
//        txtSegundoApellido.value = '';
//        txtTelefono.value = '0000000000';
//        txtCorreo.value = '';
//        selectTipoVehiculo.value = '';
//        txtVehiculoPlaca.value = '';
//        txtVehiculoModelo.value = '';
//        txtVehiculoKm.value = '';

//        // 2. Bloquear inputs (readOnly)
//        inputsCliente.forEach(input => {
//            if (input) input.readOnly = true;
//        });

//        // 3. Bloquear Select sin 'disabled' (para permitir el envío en el POST)
//        if (selectTipoDoc) {
//            selectTipoDoc.style.pointerEvents = 'none';
//            selectTipoDoc.style.backgroundColor = '#e9ecef';
//            selectTipoDoc.tabIndex = -1;
//        }

//        if (selectTipoVehiculo) {
//            selectTipoVehiculo.style.pointerEvents = 'none';
//            selectTipoVehiculo.style.backgroundColor = '#e9ecef';
//            selectTipoVehiculo.tabIndex = -1;
//        }

//        // 4. Deshabilitar botón de búsqueda
//        if (btnBuscar) btnBuscar.disabled = true;

//    } else {
//        LimpiarCamposCliente();
//        selectTipoDoc.value = selectTipoDoc_antes;
//        txtDocumento.value = txtDocumento_antes;
//        txtPrimerNombre.value = txtPrimerNombre_antes;
//        txtSegundoNombre.value = txtSegundoNombre_antes;
//        txtPrimerApellido.value = txtPrimerApellido_antes;
//        txtSegundoApellido.value = txtSegundoApellido_antes;
//        txtTelefono.value = txtTelefono_antes;
//        txtCorreo.value = txtCorreo_antes;
//        txtVehiculoPlaca.value = txtVehiculoPlaca_antes;
//        selectTipoVehiculo.value = selectTipoVehiculo_antes;        
//        txtVehiculoModelo.value = txtVehiculoModelo_antes;
//        txtVehiculoKm.value = txtVehiculoKm_antes;
//    }
//});


//function LimpiarCamposCliente() {
//    // 1. Obtener referencias
//    //const chkConsumidor = document.getElementById('chkConsumidorFinal');
//    //const selectTipoDoc = document.getElementById('TipoDocumentoId');
//    //const selectTipoVehiculo = document.getElementById('TipoVehiculoId');
//    //const btnBuscar = document.getElementById('btnBuscarCliente');

//    // Lista de todos los inputs a limpiar y habilitar
//    const idsInputs = [
//        'txtClienteDocumento',
//        'txtPrimerNombre',
//        'txtSegundoNombre',
//        'txtPrimerApellido',
//        'txtSegundoApellido',
//        'txtClienteTelefono',
//        'txtCorreo',
//        'selectTipoVehiculo',
//        'txtVehiculoPlaca',
//        'txtVehiculoModelo',
//        'txtVehiculoKm'
//    ];

//    // 2. Desmarcar checkbox si estaba activo
//    if (chkConsumidor) chkConsumidor.checked = false;

//    // 3. Limpiar y desbloquear inputs
//    idsInputs.forEach(id => {
//        const input = document.getElementById(id);
//        if (input) {
//            input.value = '';
//            input.readOnly = false;
//        }
//    });

//    // 4. Limpiar y desbloquear Select
//    if (selectTipoDoc) {
//        selectTipoDoc.value = '';
//        selectTipoDoc.style.pointerEvents = '';
//        selectTipoDoc.style.backgroundColor = '';
//        selectTipoDoc.removeAttribute('tabindex');
            
//        // Si usas Bootstrap Selectpicker, descomenta la siguiente línea:
//        // $(selectTipoDoc).selectpicker('refresh');
//    }

//    if (selectTipoVehiculo) {
//        selectTipoVehiculo.value = '';
//        selectTipoVehiculo.style.pointerEvents = '';
//        selectTipoVehiculo.style.backgroundColor = '';
//        selectTipoVehiculo.removeAttribute('tabindex');
//    }

//    // 5. Habilitar el botón de búsqueda
//    if (btnBuscar) btnBuscar.disabled = false;
//}



//// Función que calcula el valor unitario basándose en el Total / Unidades
//function calcularUnidadDesdeTotal() {
//    const total = limpiarNumero(inputPrecioVentaTotal.value);
//    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;

//    if (cantidad > 0) {
//        const precioUnidad = total / cantidad;
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precioUnidad);
//    } else {
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(0);
//    }

//    // Asegura el formato de moneda en el total
//    inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
//}


//// A. Al escribir o cambiar el valor en el TOTAL VENTA
//inputPrecioVentaTotal.addEventListener('input', function () {
//    if (chkManualTotal.checked) {
//        calcularUnidadDesdeTotal();
//    }
//});

//// A.1 Aplicar formato moneda final al perder el foco (blur)
//inputPrecioVentaTotal.addEventListener('blur', function () {
//    if (chkManualTotal.checked) {
//        const num = limpiarNumero(this.value);
//        this.value = SoloFormatoMoneda(num);
//    }
//});

//// B. Al cambiar la CANTIDAD DE UNIDADES A VENDER
//cantUnidadesVender.addEventListener('input', function () {
//    if (chkManualTotal.checked) {
//        // Si está en manual, recalcula el valor unitario dividiendo el total
//        calcularUnidadDesdeTotal();
//    } else if (typeof actualizarCalculoUnidades === 'function') {
//        // Si NO está en manual, ejecuta la lógica estándar
//        actualizarCalculoUnidades();
//    }
//});

//if (cantUnidadesVender) {
//    // Evento 'input' detecta cada tecla o cambio en el valor
//    cantUnidadesVender.addEventListener('input', () => {
//        actualizarCalculoUnidades();
//    });
//}

//function calcularVenta() {
//    const stockActual = parseInt(lblCantStockActual.getAttribute("data-valor-interno") || 0);
//    const cantUniVender = parseFloat(cantUnidadesVender.value) || 0;
//    const precio = parseCurrency(inputPrecioVentaPorUni.value);

//    let total = 0;

//    if (cantUniVender > stockActual) {
//        total = stockActual * precio;
//    } else {
//        total = cantUniVender * precio;
//    }

//    if (inputPrecioVentaPorUni) {
//        inputPrecioVentaPorUni.value = SoloFormatoMoneda(precio);
//    }

//    if (inputPrecioVentaTotal) {
//        inputPrecioVentaTotal.value = SoloFormatoMoneda(total);
//    }

//}


//function limpiarLotesCompletamente() {
//    // 1. Vaciar el arreglo de selecciones
//    lotesSeleccionados = [];

//    // 2. Obtener el contenedor principal de los lotes
//    const contenedorLotes = document.getElementById('contenedorLotes'); // ⚠️ Ajusta este ID al de tu HTML

//    if (contenedorLotes) {
//        // Destruir instancias de Tooltips para evitar fugas de memoria o globos huérfanos
//        contenedorLotes.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
//            const instance = bootstrap.Tooltip.getInstance(el);
//            if (instance) instance.dispose();
//        });

//        // 🧹 Eliminar visualmente todos los botones de lotes del DOM
//        contenedorLotes.innerHTML = '';
//    }

//    // 3. Limpiar la sección visual de Asignación/Resumen (si la tienes en un contenedor aparte)
//    const contenedorAsignacion = document.getElementById('contenedorAsignacion'); // ⚠️ Opcional
//    if (contenedorAsignacion) {
//        contenedorAsignacion.innerHTML = '';
//    }

//    // 4. Recalcular o resetear los contadores
//    if (typeof actualizarCalculoUnidades === 'function') {
//        actualizarCalculoUnidades();
//    }
//}


//// 1. Agrega el producto configurado a la tabla
//function agregarProductoACarrito(productoActual) {
//    const yaExiste = carritoVenta.some(item => String(item.codigoProducto) === String(productoActual.codigoProducto));

//    if (yaExiste) {
//        MostrarAlerta("warning", "Atención", "Este producto ya fue agregado a la lista de venta.", 4000);
//        return;
//    }

//    const cantidad = parseInt(cantUnidadesVender.value, 10) || 0;
//    const precioUnid = limpiarNumero(inputPrecioVentaPorUni.value);
//    const total = cantidad * precioUnid;

//    if (cantidad <= 0 || precioUnid <= 0) {
//        MostrarAlerta("warning", "Atención", "Ingrese una cantidad y precio válidos.", 4000);
//        return;
//    }

//    if (!lotesSeleccionados || lotesSeleccionados.length === 0) {
//        MostrarAlerta("warning", "Atención", "Debe seleccionar al menos un lote para este producto.", 4000);
//        return;
//    }

//    // Calculamos la distribución real de unidades tomadas de cada lote
//    const distribucionResult = calcularDistribucionLotes(cantidad);

//    if (distribucionResult.unidadesPendientes > 0) {
//        MostrarAlerta("warning", "Atención", `Aún faltan ${distribucionResult.unidadesPendientes} unidades por cubrir. Seleccione más lotes.`, 5000);
//        return;
//    }

//    // Mapeamos la distribución obtenida relacionándola con lotesActuales
//    const lotesEstandarizados = distribucionResult.distribucion
//        .filter(item => item.tomadas > 0)
//        .map(item => {
//            // Buscar la información completa del lote en la lista global
//            const loteObj = lotesActuales.find(l => String(l.idLote ?? l.IdLote ?? l.id) === String(item.loteId));

//            const id = item.loteId;

//            // 🔍 Evaluamos todas las posibles propiedades del lote
//            let refLote = loteObj?.referencia || loteObj?.Referencia || loteObj?.nombreLote || loteObj?.codigoLote || loteObj?.numLote;

//            // Si dice "NO APLICA", está nulo o vacío, mostramos "Lote X" (con el ID del lote)
//            if (!refLote || refLote.toString().trim().toUpperCase() === 'NO APLICA' || refLote.toString().trim() === '') {
//                refLote = `Lote ${id}`;
//            }

//            return {
//                idLote: id,
//                referencia: refLote,
//                cantidad: item.tomadas
//            };
//        });

//    let nomProducto = (productoActual.nombreProducto || '').toUpperCase().trim();
//    let marcaProducto = (productoActual.nombreMarca || '').toUpperCase().trim();
//    let colorProducto = (productoActual.nombreColor || '').toUpperCase().trim();
//    let referenciaProducto = (productoActual.referencia || '').toUpperCase().trim();

//    let detalleProducto = [nomProducto, marcaProducto, colorProducto, referenciaProducto]
//        .filter(val => val !== '' && val !== 'NO APLICA')
//        .join(' - ');

//    const detalleItem = {
//        codigoProducto: productoActual.codigoProducto,
//        nombreProducto: detalleProducto,
//        cantidad: cantidad,
//        precioUnitario: precioUnid,
//        total: total,
//        lotes: lotesEstandarizados
//    };

//    carritoVenta.push(detalleItem);
//    renderizarTablaVenta();
//    LimpiarCamposVenta();
//    if (validarCodigo) validarCodigo.value = '';
//}



//// 2. Dibuja las filas en el HTML y recalcula el Gran Total
//function renderizarTablaVenta() {
//    const tbody = document.getElementById('tbodyDetalleVenta');
//    const lblTotalPagar = document.getElementById('lblTotalPagar');

//    if (!tbody) return;

//    if (carritoVenta.length === 0) {
//        tbody.innerHTML = `
//            <tr id="trFilaVacia">
//                <td colspan="6" class="text-center text-muted py-4">No hay productos agregados a la venta.</td>
//            </tr>`;
//        if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(0);
//        return;
//    }

//    let html = '';
//    let granTotal = 0;

//    carritoVenta.forEach((item, index) => {
//        granTotal += item.total;

//        // Formatear texto con los lotes asignados
//        const textoLotes = (item.lotes && item.lotes.length > 0)
//            ? item.lotes.map(l => `${l.referencia} (${l.cantidad} und)`).join(', ')
//            : 'Sin lote asignado';

//        html += `
//            <tr>
//                <td class="text-center fw-bold fs-7">${item.codigoProducto}</td>
//                <td>
//                    <div class="fw-bold">${item.nombreProducto}</div>
//                    <small class="text-muted fs-8 d-block">
//                        Lotes: ${textoLotes}
//                    </small>
//                </td>
//                <td class="text-center fw-bold">${item.cantidad}</td>
//                <td class="text-end">${SoloFormatoMoneda(item.precioUnitario)}</td>
//                <td class="text-end fw-bold text-success">${SoloFormatoMoneda(item.total)}</td>
//                <td class="text-center">
//                    <button class="btn btn-outline-danger btn-sm border-0" onclick="eliminarItemCarrito(${index})">
//                        <i class="fa-solid fa-trash-arrow-up"></i> Quitar
//                    </button>
//                </td>
//            </tr>`;
//    });

//    tbody.innerHTML = html;
//    if (lblTotalPagar) lblTotalPagar.value = SoloFormatoMoneda(granTotal);
//}

//// 3. Elimina un ítem de la tabla
//function eliminarItemCarrito(index) {
//    carritoVenta.splice(index, 1);
//    renderizarTablaVenta();
//    calcularTotalPagado_Y_Restante(null, null, null);
//}

//// 4. Envía la venta completa y sus lotes al servidor
//async function registrarVenta() {
//    if (carritoVenta.length === 0) {
//        MostrarAlerta("warning", "Venta Vacía", "Agregue al menos un producto a la tabla.", 4000);
//        return;
//    }

//    const payloadVenta = {
//        totalVenta: carritoVenta.reduce((acc, item) => acc + item.total, 0),
//        detalles: carritoVenta
//    };

//    try {
//        const response = await fetch('/Ventas/RegistrarVenta', {
//            method: 'POST',
//            headers: { 'Content-Type': 'application/json' },
//            body: JSON.stringify(payloadVenta)
//        });

//        if (response.ok) {
//            MostrarAlerta("success", "Éxito", "La venta y el descuento de lotes se registraron correctamente.", 5000);
//            carritoVenta = [];
//            renderizarTablaVenta();
//        } else {
//            const errorMsg = await response.text();
//            MostrarAlerta("error", "Error en Venta", errorMsg || "No se pudo registrar la venta.", 5000);
//        }
//    } catch (error) {
//        console.error('Error enviando la venta:', error);
//        MostrarAlerta("error", "Conexión", "Error de red al intentar procesar la venta.", 5000);
//    }
//}


//// Supongamos que esta es tu función donde cargas los datos del producto
//function alCargarProductoExitoso(producto) {
//    // 1. Guardas el producto actual en la variable global
//    productoCargadoActual = producto;

//    // 2. Muestras sus detalles en la interfaz
//    document.getElementById('lblMarcaProducto').textContent = producto.marca;
//    document.getElementById('lblNombreProducto').textContent = producto.nombre;
//    // ... resto de tu lógica para mostrar stock, lotes, etc.
//}

//// Función ejecutada por el botón "Agregar a la Venta"
//function prepararYAgregarProducto() {
//    if (!productoCargadoActual) {
//        MostrarAlerta("warning", "Atención", "Primero debe buscar y seleccionar un producto.", 4000);
//        return;
//    }

//    // Ejecuta la función que inserta el ítem en la tabla y recalcula totales
//    agregarProductoACarrito(productoCargadoActual);
//    calcularTotalPagado_Y_Restante(null, null, null);
//}


////$('.selectpicker').selectpicker('refresh');


//function BuscarCliente() {
//    let clienteEncontrado = false;
//    let documento = selectTipoDoc.value;
//    let tipoDocumento = txtDocumento.value;
     

//    if (!documento || tipoDocumento.length < 6) {
//        console.log("El campo de código está vacío.");
//        MostrarAlerta("warning", "Documento incompleto", "Agregue un número de codumento valido.", 5000);
//        return;
//    }

//    try {
//        const response = await fetch(`/Usuarios/ObtenerUsuario?filtroId=${documento}`);

//        if (response.ok) {
//            const clienteActual = await response.json();

//            if (clienteActual && Object.keys(clienteActual).length > 0) {
//                clienteEncontrado = true;

//                if (selectTipoDoc) selectTipoDoc.value = clienteActual.tipoDocumentoId;
//                if (txtDocumento) txtDocumento.value = clienteActual.documento;
//                if (txtPrimerNombre) txtPrimerNombre.value = clienteActual.primerNombre;
//                if (txtSegundoNombre) txtSegundoNombre.value = clienteActual.segundoNombre;
//                if (txtPrimerApellido) txtPrimerApellido.value = clienteActual.primerApellido;
//                if (txtSegundoApellido) txtSegundoApellido.value = clienteActual.segundoApellido;
//                if (txtTelefono) txtTelefono.value = clienteActual.telefono;
//                if (txtCorreo) txtCorreo.value = clienteActual.correo;
//                //if (txtVehiculoPlaca) txtVehiculoPlaca.value = clienteActual.vehiculoPlaca;
//                //if (selectTipoVehiculo) selectTipoVehiculo.value = clienteActual.tipoVehiculoId;
//                //if (txtVehiculoModelo) txtVehiculoModelo.value = clienteActual.vehiculoModelo   ;
//                //if (txtVehiculoKm) txtVehiculoKm.value = clienteActual.vehiculoKm;
                                
//            } else {
//                console.log("Cliente no encontrado o datos vacíos.");
//                MostrarAlerta("warning", "Cliente", "Datos vacíos del usuario.", 5000);
//            }
//        } else {
//            console.log("No se encontró en la respuesta de la API el usuario: ", documento);
//            MostrarAlerta("warning", "Cliente", "usuario no encontrado.", 5000);
//        }
//    } catch (error) {
//        console.error('Error al buscar cliente:', error);
//        MostrarAlerta("warning", "Cliente", "Error al buscar usuario: " + error, 5000);
//    }
//}