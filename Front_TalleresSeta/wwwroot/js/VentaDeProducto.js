// Variable global o de ámbito de la venta
let lotesActuales = null;
let lotesSeleccionados = []; // Guarda los IDs de los lotes activos en orden de clic

const visualizaMarcaProducto = document.getElementById('lblMarcaProducto');
const visualizaNombreProducto = document.getElementById('lblNombreProducto');
const visualizaReferenciaProducto = document.getElementById('lblReferenciaProducto');
const visualizaColorProducto = document.getElementById('lblColorProducto');
const lblCantStockActual = document.getElementById('lblCantStockActual');
let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
const cantUnidadesVender = document.getElementById('validarCantidadVendidos');

let ImagenProducto = document.getElementById('ImagenProducto');
let previewImagenProducto = document.getElementById('previewImagenProducto');

/**
 * 1. Carga los lotes desde el Backend filtrando por código de producto
 */
async function CargarLotesProductoVenta(producto) {
    if (!producto || !producto.codigoProducto) {
        console.warn('El producto ingresado no contiene un código válido.');
        return;
    }

    // CORREGIDO: Limpiar arreglo en lugar de .clear()
    lotesSeleccionados = [];

    try {
        const codigo = encodeURIComponent(producto.codigoProducto);
        const response = await fetch(`/InventarioLotes/MostrarLotesPorProducto?filtroId=${codigo}`);

        if (response.ok) {
            const lotes = await response.json();

            lotesActuales = lotes;

            const contenedor = document.getElementById('contenedorDetallesProducto');
            if (contenedor) {
                contenedor.classList.remove('d-none');
                contenedor.removeAttribute('aria-hidden');
            }

            renderizarLotes(lotes || []);
        } else {
            console.error('Error al consultar lotes:', response.statusText);
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
        return;
    }

    lotes.forEach(lote => {
        const idLote = lote.idLote ?? lote.IdLote;
        const stock = lote.cantRestante ?? lote.CantRestante ?? 0;
        const precioVenta = lote.precioVentaXuni ?? lote.PrecioVentaXuni ?? 0;
        const loteIdSanitizado = String(idLote).replace(/[^\w-]/g, '');

        const btnLote = document.createElement('button');
        btnLote.type = 'button';
        btnLote.className = 'btn btn-outline-primary btn-sm rounded-pill position-relative me-1 mb-1 btn-lote';
        btnLote.dataset.loteId = loteIdSanitizado;
        btnLote.dataset.stock = stock;
        btnLote.dataset.precio = precioVenta;

        btnLote.innerHTML = `
            Lote: <strong>#${idLote}</strong> 
            <span class="badge bg-secondary ms-1">${stock} und</span>
        `;

        btnLote.addEventListener('click', () => toggleSeleccionLote(loteIdSanitizado, btnLote));
        contenedor.appendChild(btnLote);
    });
    actualizarCalculoUnidades();
}

/**
 * 3. Manejo de selección/deselección de lotes manteniendo orden de clic
 */
function toggleSeleccionLote(loteId, elementoHtml) {
    const index = lotesSeleccionados.indexOf(loteId);

    if (index > -1) {
        lotesSeleccionados.splice(index, 1);
        elementoHtml.classList.remove('btn-primary', 'active');
        elementoHtml.classList.add('btn-outline-primary');
    } else {
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

    // 1. Validar que sea un número válido y mayor a 0
    if (isNaN(cantidadAVender) || cantidadAVender <= 0) {
        cantidadAVender = 0;
    }

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (!resumen) return;

    if (!lotesActuales || lotesActuales.length === 0) {
        resumen.innerHTML = '<span class="text-danger fs-7">No hay lotes disponibles en stock.</span>';
        return;
    }

    // 2. Calcular el STOCK TOTAL GLOBAL de todos los lotes
    const stockTotalGlobal = lotesActuales.reduce((suma, lote) => {
        return suma + (lote.cantRestante ?? lote.stock ?? 0);
    }, 0);

    // 3. CONTROL DE TOPE MÁXIMO: Si pide más del stock total, se corrige automáticamente
    if (cantidadAVender > stockTotalGlobal) {
        cantidadAVender = stockTotalGlobal;
        if (inputCantidad) {
            inputCantidad.value = stockTotalGlobal; // Actualiza visualmente el input a la cantidad real permitida
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

    // 4. Ejecutar algoritmo de distribución con la cantidad YA CORREGIDA
    const resultado = calcularDistribucionLotes(cantidadAVender);

    let htmlResumen = '<div class="d-flex flex-wrap align-items-center gap-1 mb-1"><span class="fw-bold me-1 text-dark fs-7">Asignación:</span>';

    resultado.distribucion.forEach(item => {
        const badgeColor = item.tomadas > 0 ? 'bg-light text-primary border-primary' : 'bg-light text-muted border-secondary';

        htmlResumen += `
            <span class="badge ${badgeColor} border fs-7 me-1">
                Lote #${item.codigo}: <strong class="text-dark">${item.tomadas} und</strong>
            </span>
        `;
    });

    htmlResumen += '</div>';

    // 5. Mensajes de estado
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

                if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = producto.precioVentaXuni;

                if (typeof formatoMoneda === 'function') {
                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');
                }

                CargarLotesProductoVenta(producto);

                if (typeof CargarImagenBase64 === 'function') {
                    CargarImagenBase64(previewImagenProducto, producto.imagenProducto);
                }

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

    const iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
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

    if (lblCantStockActual) lblCantStockActual.setAttribute("data-valor-interno", '');
    if (inputPrecioVentaPorUni) inputPrecioVentaPorUni.value = '';
    if (cantUnidadesVender) cantUnidadesVender.value = '';

    if (visualizaMarcaProducto) visualizaMarcaProducto.innerText = '';
    if (visualizaNombreProducto) visualizaNombreProducto.innerText = '';
    if (visualizaReferenciaProducto) visualizaReferenciaProducto.innerText = '';
    if (visualizaColorProducto) visualizaColorProducto.innerText = '';

    if (previewImagenProducto) {
        previewImagenProducto.style.display = 'none';
        previewImagenProducto.value = null;
    }
    if (ImagenProducto) ImagenProducto.value = null;

    const resumen = document.getElementById('resumenSeleccionLotes');
    if (resumen) resumen.innerHTML = '';
}

if (cantUnidadesVender) {
    // Evento 'input' detecta cada tecla o cambio en el valor
    cantUnidadesVender.addEventListener('input', () => {
        actualizarCalculoUnidades();
    });
}