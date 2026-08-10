document.addEventListener("DOMContentLoaded", () => {
    let validarCodigo = document.getElementById("validarCodigo");
    const contenedor = document.getElementById("contenedor");
    const marcoEscaneo = document.getElementById("marcoEscaneo");
    const btnCapturar = document.getElementById("btnCapturar");
    const CancelarEscaneo = document.getElementById("CancelarEscaneo");
    const btnGenerar = document.getElementById("btnGenerar");
    const beep = document.getElementById("beep");
    const barcodeCanvas = document.getElementById("barcodeCanvas");
    const barcodeImage = document.getElementById("barcodeImage");
    const btnDescargar = document.getElementById("btnDescargar");
    let imagenCodigoDeBarras = document.getElementById("ImagenCodigoDeBarras");
    let consecutivoCodBarras = document.getElementById("consecutivoCodBarras");
    let selectProveedor = document.getElementById("validarInventarioProveedorId");
    let selectMarca = document.getElementById("validarInventarioMarcaId");
    let selectCategoria = document.getElementById("idCategoria");
    let selectHomologado = document.getElementById("validarHomologado");
    let selectStockMinimo = document.getElementById("validarStockMinimo");
    let selectNombreProducto = document.getElementById("validarNombre");
    let selectReferencia = document.getElementById("validarReferencia");
    let selectColor = document.getElementById("validarColor");
    let selectVehiculoAsociado = document.getElementById("validarVehiculoAsociado");
    let selectPosicionProducto = document.getElementById("validarPosicionProducto");
    let selectUbicacionProducto = document.getElementById("validarUbicacionProducto");
    let selectidMedida_c = document.getElementById("idMedida_c");
    let selectUnidadMedida = document.getElementById("validarUnidadMedida");
    let selectDetalle = document.getElementById("validarDetalle");

    let escaneoActivo = false;
    let codeReader = null;
    let fuePresionadoGenerarCodigo = false;
    let fuePresionadoCapturarCodigo = false;
    let limpiarCampos = true;
    let decodeProcess = null;

    let inputImagenProducto = document.getElementById('inputImagenProducto');
    let ImagenProducto = document.getElementById('ImagenProducto');
    let previewImagenProducto = document.getElementById('previewImagenProducto');
    let imagenProductoBase64 = null;

    
    // 🚀 Función para activar la cámara solo cuando se necesite
    async function iniciarEscaneo() {
        try {

            if (escaneoActivo) return;

            escaneoActivo = true;
            contenedor.style.display = "flex"; // 🔥 Ahora se muestra al capturar
            //contenedor.style.display = "block";
            marcoEscaneo.classList.remove("detectado"); // Reiniciar el marco a rojo

            // Crear lector
            codeReader = new ZXingBrowser.BrowserMultiFormatReader();

            // Obtener dispositivos de video
            const devices = await ZXingBrowser.BrowserCodeReader.listVideoInputDevices();

            // Buscar cámara trasera (que contenga "back" o "rear" en la etiqueta)
            let selectedDeviceId = devices[0].deviceId;
            for (let device of devices) {
                if (/back|rear/i.test(device.label)) {
                    selectedDeviceId = device.deviceId;
                    break;
                }
            }
            // Iniciar escaneo
            decodeProcess = await codeReader.decodeOnceFromVideoDevice(selectedDeviceId, 'video');
            if (decodeProcess) {
                //const codigo = decodeProcess.text.replace(/^0+/, '');
                //let codigo = decodeProcess.text;
                let codigo = decodeProcess.text.trim();
                const formato = decodeProcess.barcodeFormat; // Aquí obtenemos el formato
                validarCodigo.value = codigo;
                beep.play();
                marcoEscaneo.classList.add("detectado");
                limpiarCampos = true;
                GenerarImagen("CAPTURA", limpiarCampos);
                detenerEscaneo();
            } else {
                return
            }
        } catch (error) {
            console.error("Error al iniciar ZXingBrowser:", error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se pudo acceder a la cámara.',
                timer: 5000, // ⏳ Se cierra automáticamente en 5 segundos
                showConfirmButton: false
            });
            detenerEscaneo();
        }
    }

    // Detener escaneo y limpiar
    function detenerEscaneo() {
        if (!escaneoActivo) return;
        escaneoActivo = false;

        try {
            // Detener el proceso de decodificación si está en curso
            if (decodeProcess) {
                decodeProcess = null;
            }

            // Detener el flujo de video y liberar la cámara
            const videoElement = document.getElementById('video');
            if (videoElement && videoElement.srcObject) {
                const stream = videoElement.srcObject;
                const tracks = stream.getTracks();
                tracks.forEach(track => track.stop());
                videoElement.srcObject = null;
            }

            // Ocultar el contenedor de video
            const contenedor = document.getElementById('contenedor');
            contenedor.style.display = 'none';
            CancelarEscaneo.style.display = 'none';
            // Limpiar el lector y liberar recursos
            //if (await codeReader.reset()) {
            codeReader = null;
            //}
        } catch (error) {
            console.error('Error al detener el escaneo:', error);
        }
    }

    async function GenerarImagen(tipoSecuenciaCodigo, limpiarCampos) {
        try {
            const response = await fetch("/Funciones/ObtenerUltimoCodigoDeBarrasGenerado"); // Ajusta la URL si es diferente            
            if (!response.ok) throw new Error("Error al consultar el último ID");

            const data = await response.text(); // obtiene el valor como texto
            const ultimoId = parseInt(data) || 0; // Si no existe, iniciar desde 0
            let secuencia = ultimoId + 1;
            let codigoAleatorio = null;
            let codigoGenerado = null;
            consecutivoCodBarras.value = 0;

            // Generar código aleatorio de 3 dígitos
            if (tipoSecuenciaCodigo === "ALEATORIO") {
                codigoAleatorio = Math.floor(100 + Math.random() * 900).toString();

                const fechaActual = new Date();
                let anno = fechaActual.getFullYear().toString().slice(-2);
                let mes0 = fechaActual.getMonth() + 1;
                let dia = fechaActual.getDay();
                let mes = mes0.toString().padStart(2, '0');

                var idProveedor = document.querySelector("#validarInventarioProveedorId").value;
                var idMarca = document.querySelector("#validarInventarioMarcaId").value;
                var idCategoria = document.querySelector("#idCategoria").value;

                let proveedor = (idProveedor && idProveedor !== "") ? parseInt(idProveedor) : 0;
                let marca = (idMarca && idMarca !== "") ? parseInt(idMarca) : 0;
                let categoria = (idCategoria && idCategoria !== "") ? parseInt(idCategoria) : 0;

                /* El código de barras creado se compone de: (?-P?M?C?-?????)
                   secuencia: último número registrado en la DB con Generación de código de barras 
                   P: el id del proveedor, interno
                   M: el id de la marca, interno 
                   C: el id de la categoria, interno                   
                   codigoAleatorio: compuesto por 3 cifras aleatorias 
                   0 una cifra eqyivalente al día
                   00 dos cifras equivalente al mes
                   00 dos cifras equivalente al año*/
                codigoGenerado = `${secuencia}-P${proveedor}M${marca}C${categoria}-${codigoAleatorio}-${dia}${mes}${anno}`;
                consecutivoCodBarras.value = secuencia;
            }
            else if (tipoSecuenciaCodigo === "INGRESADO") {
                codigoGenerado = validarCodigo.value;
            }
            else if (tipoSecuenciaCodigo === "CAPTURA") {
                codigoGenerado = validarCodigo.value;
            } else {
                codigoGenerado = validarCodigo.value;
            }
                       

            // 🎯 Generar código de barras
            if (codigoGenerado.length > 0) {
                codigoGenerado = codigoGenerado.replace(/\s+/g, '');
                validarCodigo.value = codigoGenerado;
                CargarProductoExiste(limpiarCampos);

                // Obtener el formato compatible con JsBarcode
                let formatoCodBarras = "CODE128"; // Por defecto CODE128

                JsBarcode(barcodeCanvas, codigoGenerado, {
                    format: formatoCodBarras,
                    displayValue: true,
                    lineColor: "#000",
                    width: 2,       // Más grueso: 2.5 píxeles por barra
                    height: 150,       // Más alto: 80 píxeles de altura
                    fontSize: 40,     // Tamaño del texto debajo del código
                    margin: 30        // Espacio alrededor del código
                });

                // 📸 Mostrar imagen generada
                barcodeImage.src = barcodeCanvas.toDataURL("image/png");
                barcodeImage.style.display = "block";

                guardarImagenCodigoBarras(barcodeCanvas, barcodeImage);

                // 📝 Activar descarga
                btnDescargar.style.display = "block";
                btnDescargar.onclick = () => descargarImagen(codigoGenerado);                
            } else {
                barcodeImage.style.display = "none";
                btnDescargar.style.display = "none";
                //fuePresionadoGenerarCodigo == true;
            }
        } catch (error) {
            //fuePresionadoGenerarCodigo == true;
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se pudo generar el código. Intenta de nuevo.',
                timer: 5000, // ⏳ Se cierra automáticamente en 5 segundos
                showConfirmButton: false
            });
        }
    }

    async function CargarProductoExiste(limpiarCampos) {
        let codigo = document.getElementById("validarCodigo").value;
        let productoEncontrado = false;

        if (limpiarCampos == true) {
            LimpiarCampos();
        }

        if (!codigo) {
            // Si no hay código, los campos ya están limpios, así que salimos.
            console.log("El campo de código está vacío.");
            return;
        }

        try {
            const response = await fetch(`/Funciones/CargarProductoExiste?filtroId=${codigo}`);

            if (response.ok) {
                const producto = await response.json();

                // Asegúrate de que el producto se encontró y no es un objeto vacío/nulo
                // Tu API debería devolver un 404 o un objeto vacío si no hay producto.
                // Si devuelve un 200 OK con un objeto vacío o null, maneja eso aquí.
                if (producto && Object.keys(producto).length > 0) { // Verifica si el objeto producto no está vacío
                    productoEncontrado = true;

                    // 2. Llenar los campos con los datos del producto (solo si se encontró)
                    selectHomologado.checked = producto.homologado || '';
                    selectStockMinimo.value = producto.stockMinimo || '';
                    selectNombreProducto.value = producto.nombreProducto || '';
                    selectReferencia.value = producto.referencia || '';
                    selectColor.value = producto.colores_c.nombreColor || '';
                    selectVehiculoAsociado.value = producto.vehiculoAsociado || '';
                    selectPosicionProducto.value = producto.posicionProducto || '';
                    selectUbicacionProducto.value = producto.ubicacionProducto || '';
                    selectidMedida_c.value = producto.unidadMedidas.medidas.nombreMedida || '';
                    selectUnidadMedida.value = producto.unidadMedidas.unidadMedida || '';
                    selectDetalle.value = producto.detalle || '';

                    CargarImagenBase64(previewImagenProducto, producto.imagenProducto);
                    ImagenProducto.value = producto.imagenProducto;

                    // Para los SelectList (elementos <select>)
                    seleccionarPorTexto("validarInventarioProveedorId", producto.inventarioProveedores.razonSocial);
                    seleccionarPorTexto("validarInventarioMarcaId", producto.inventarioMarcas.nombreMarca);
                    seleccionarPorTexto("idCategoria", producto.inventarioSubCategorias.inventarioCategorias.nombreCategoria);
                    seleccionarPorTexto("validarInventarioSubCategoriaId", producto.inventarioSubCategorias.nombreSubCategoria);
                    seleccionarPorTexto("validarColor", producto.colores_c.nombreColor);
                    seleccionarPorTexto("idMedida_c", producto.unidadMedidas.medidas.nombreMedida);
                    seleccionarPorTexto("validarUnidadMedida", producto.unidadMedidas.unidadMedida);

                    console.log("Producto cargado exitosamente.");
                } else {
                    console.log("Producto no encontrado o datos vacíos.");                    
                }
            } else if (response.status === 404) {
                console.log("Producto no encontrado. Código de estado: 404.");                
            } else {
                console.log("No se encontró en la respuesta de la API el producto: ", codigo);
            }
        } catch (error) {
            console.error('Error al buscar producto:', error);
            //Swal.fire({
            //    icon: 'error',
            //    title: 'Error de conexión',
            //    text: 'No se pudo conectar con el servidor para buscar el producto.',
            //    timer: 5000,
            //    showConfirmButton: false
            //});
            // Los campos ya están limpios por LimpiarCampos()
        }

        if (productoEncontrado && ImagenProducto.value != null) {
            previewImagenProducto.style.display = 'block';
            iconoPreviewImagenProducto.style.display = 'none';
        } else if (productoEncontrado == false)  {
            previewImagenProducto.style.display = 'none';
            iconoPreviewImagenProducto.style.display = 'block';
        }
    }

    function guardarImagenCodigoBarras(barcodeCanvas, ImagenCodigoDeBarras) {
        try {
            let imgData = barcodeCanvas.toDataURL("image/png"); // Convertir canvas a Base64

            // Remover el encabezado "data:image/png;base64," para solo guardar la base64 pura
            let base64Data = imgData.replace(/^data:image\/(png|jpg);base64,/, "");

            // Asignar la base64 al campo oculto
            imagenCodigoDeBarras.value = base64Data;
        } catch (error) {
            console.warn("Error al guardar el código de barras, con el error: ", error);
        }
    }

    function CargarImagenBase64(canvaImagen, imagenBase) {                
        if (imagenBase && canvaImagen) {
            let existingBase64 = imagenBase; // Obtiene la cadena Base64 del input hidden
            if (existingBase64) {
                let imageUrl = `data:image/png;base64,${existingBase64}`;
                canvaImagen.style.display = 'block'; // Muestra la imagen
                canvaImagen.src = imageUrl;                                
            } else {
                canvaImagen.style.display = 'none'; // Asegúrate de que esté oculta si no hay imagen
                console.log("No hay imagen Base64 existente en el hidden input.");
            }
        } else {
            console.warn(`Elementos HTML 'ImagenProducto' o ' ${canvaImagen} ' no encontrados.`);
        }
    }

    function descargarImagen(nombreArchivo) {
        const link = document.createElement("a");
        link.href = barcodeCanvas.toDataURL("image/png");
        link.download = `CodigoDeBarras_${nombreArchivo}.png`;
        link.click();
    }

    function seleccionarPorTexto(selectId, texto) {
        const select = document.getElementById(selectId);
        for (let option of select.options) {
            if (option.text.trim() === texto.trim()) {
                select.value = option.value;

                // Si estás usando Bootstrap-select, refresca visualmente
                if ($(select).hasClass('selectpicker')) {
                    $(select).selectpicker('val', option.value);  // Establece el valor visualmente
                    $(select).selectpicker('refresh');            // Refresca el componente
                }

                break;
            }
        }
    }

    function limpiarSeleccionSelect(selectId) {
        const select = document.getElementById(selectId);
        if (!select) return;

        // Quitar la selección
        select.selectedIndex = -1;

        // Si usas Bootstrap-select (selectpicker), quita valor visual y refresca
        if ($(select).hasClass('selectpicker')) {
            $(select).selectpicker('val', '');
            $(select).selectpicker('refresh');
        }
    }

    function LimpiarCampos() {
        selectHomologado.checked = false;
        selectStockMinimo.value = 2 || '';
        selectNombreProducto.value = '';
        selectReferencia.value = '';
        selectColor.value = '';
        selectVehiculoAsociado.value = '';
        selectPosicionProducto.value = '';
        selectUbicacionProducto.value = '';
        selectidMedida_c.value = '';
        selectUnidadMedida.value = '';
        selectDetalle.value = '';
        inputImagenProducto.value = '';

        limpiarSeleccionSelect("validarInventarioProveedorId");
        limpiarSeleccionSelect("validarInventarioMarcaId");
        limpiarSeleccionSelect("idCategoria");
        limpiarSeleccionSelect("validarInventarioSubCategoriaId");
        limpiarSeleccionSelect("validarColor");
        limpiarSeleccionSelect("idMedida_c");
        limpiarSeleccionSelect("validarUnidadMedida");

        previewImagenProducto.style.display = 'none';
        previewImagenProducto.src = null;
        ImagenProducto.value = null;
    }

    if (inputImagenProducto && previewImagenProducto) {
        inputImagenProducto.addEventListener('change', function (e) {
            if (e.target.files && e.target.files[0]) {
                const file = e.target.files[0];
                const reader = new FileReader();

                reader.onload = function (event) {
                    if (event.target.result != null) {
                        // 1. Mostrar la imagen en la vista previa
                        previewImagenProducto.src = event.target.result;
                        previewImagenProducto.style.display = 'block';

                        imagenProductoBase64 = event.target.result.split(',')[1];
                        ImagenProducto.value = imagenProductoBase64;
                        console.log("Imagen convertida a Base64 y almacenada.");
                        iconoPreviewImagenProducto.style.display = 'none';
                    } else {
                        iconoPreviewImagenProducto.style.display = 'block';
                        previewImagenProducto.style.display = 'none';
                    }
                };
                reader.readAsDataURL(file); // Lee el archivo como una URL de datos (Base64)
            } else {
                // Si no se selecciona ningún archivo, limpiar la vista previa y la variable Base64
                previewImagenProducto.src = '#';
                previewImagenProducto.style.display = 'none';
                iconoPreviewImagenProducto.style.display = 'block';
                imagenProductoBase64 = null;
                console.log("No se seleccionó ninguna imagen. Variable Base64 limpiada.");
            }
        });
    } else {
        console.warn("Elementos 'ImagenProducto' o 'previewImagenProducto' no encontrados en el DOM.");
    }


    window.onload = function () {
        if (!validarCodigo.value == '') {
            let movimientoCodBarras = null;
            if (fuePresionadoGenerarCodigo == true) {
                movimientoCodBarras = "ALEATORIO";
            } else if (fuePresionadoCapturarCodigo == true) {
                movimientoCodBarras = "CAPTURAR";
            }
            limpiarCampos = true;
            GenerarImagen(movimientoCodBarras, limpiarCampos);
        }
    };

    validarCodigo.addEventListener("input", () => {
        //Limpiar basura de los campos
        validarCodigo.value = validarCodigo.value.replace(/[^a-zA-Z0-9\-\/\*]/g, '');
        validarCodigo.value = validarCodigo.value.toUpperCase();

        fuePresionadoCapturarCodigo = false;
        fuePresionadoGenerarCodigo = false;
        limpiarCampos = true;
        GenerarImagen("INGRESADO", limpiarCampos);
    });

    btnCapturar.addEventListener("click", (event) => {
        event.preventDefault();
        fuePresionadoCapturarCodigo = true;
        fuePresionadoGenerarCodigo = false;
        iniciarEscaneo();

        CancelarEscaneo.style.display = 'block';
        btnCapturar.style.display = 'none';
    });

    CancelarEscaneo.addEventListener("click", (event) => {
        event.preventDefault();
        fuePresionadoCapturarCodigo = false;
        fuePresionadoGenerarCodigo = false;
        btnCapturar.style.display = 'block';
        CancelarEscaneo.style.display = 'none';
        detenerEscaneo();
    });

    btnGenerar.addEventListener("click", (event) => {
        event.preventDefault(); // ❌ Evita que el formulario se envíe
        fuePresionadoGenerarCodigo = true;
        fuePresionadoCapturarCodigo = false;
        limpiarCampos = true;
        GenerarImagen("ALEATORIO", limpiarCampos);
    });

    btnDescargar.addEventListener("click", (event) => {
        event.preventDefault(); // ❌ Evita que el formulario se envíe
    });

    selectProveedor.addEventListener("change", (event) => {
        limpiarCampos = false;
        if (fuePresionadoGenerarCodigo == true) {
            GenerarImagen("ALEATORIO", limpiarCampos);
        }
    });

    selectMarca.addEventListener("change", (event) => {
        limpiarCampos = false;
        if (fuePresionadoGenerarCodigo == true) {
            GenerarImagen("ALEATORIO", limpiarCampos);
        }
    });

    selectCategoria.addEventListener("change", (event) => {
        limpiarCampos = false;
        if (fuePresionadoGenerarCodigo == true) {
            GenerarImagen("ALEATORIO", limpiarCampos);
        }
    });

});