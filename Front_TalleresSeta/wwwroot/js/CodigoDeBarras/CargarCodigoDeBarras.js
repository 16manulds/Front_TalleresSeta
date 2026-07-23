document.addEventListener("DOMContentLoaded", () => {

    let validarCodigo = document.getElementById("validarCodigo");
    const barcodeCanvas = document.getElementById("barcodeCanvas");
    const barcodeImage = document.getElementById("barcodeImage");
    let ImagenCodigoDeBarras = document.getElementById("ImagenCodigoDeBarras");
    const btnDescargar = document.getElementById("btnDescargar");

    let ImagenProducto = document.getElementById('ImagenProducto');
    let previewImagenProducto = document.getElementById('previewImagenProducto');
    let iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
    let inputImagenProducto = document.getElementById('inputImagenProducto');
    let imagenProductoBase64 = null;

    const contenedor = document.getElementById("contenedor");
    const marcoEscaneo = document.getElementById("marcoEscaneo");
    const btnCapturar = document.getElementById("btnCapturar");
    const CancelarEscaneo = document.getElementById("CancelarEscaneo");
    const beep = document.getElementById("beep");

    const lblCargarNombreProducto = document.getElementById('lblCargarNombreProducto');
    const cargarNombreProveedor = document.getElementById('CargarNombreProveedor');
    const cargarNombreMarca = document.getElementById('CargarNombreMarca');
    const cargarNombreCategoria = document.getElementById('CargarNombreCategoria');
    const cargarNombreSubCategoria = document.getElementById('CargarNombreSubCategoria');
    const cargarColor = document.getElementById('CargarColor');
    const cargarMedida = document.getElementById('CargarMedida');
    const cargarUnidadMedida = document.getElementById('CargarUnidadMedida');
    const cargarTaller = document.getElementById('CargarTaller');
    const cargarTallerId = document.getElementById('cargarTallerId');

    let inputPrecioVentaPorUni = document.getElementById('validarPrecioVentaPorUni');
    //let inputPrecioVentaTotal = document.getElementById('validarPrecioVentaTotal');
    //let inputPrecioVentaTotalPagado = document.getElementById('validarPrecioTotalPagado');

    const cargarNombreProducto = document.getElementById('cargarNombreProducto');
    const cargarUnidadesStock = document.getElementById('cargarUnidadesStock'); // cantidad en stock
    const lblCantStockActual = document.getElementById('lblCantStockActual'); // cantidad en stock
    const cantUnidadesVender = document.getElementById('validarCantidadVendidos'); //cantidad a vender
    //const valor = labelNombre.getAttribute("data-valor-interno");


    let escaneoActivo = false;
    let codeReader = null;
    let limpiarCampos = true;
    let decodeProcess = null;

    let currentUrl = document.URL; // capturar la ruta desde donde se llama el JS


    if (!currentUrl.includes('/InventarioSalidaProductos/Create')) {

        window.onload = function () {
            if (ImagenProducto != null) {
                CargarImagenBase64(previewImagenProducto, ImagenProducto.value); // Cargar imagen del producto
            }
            if (ImagenCodigoDeBarras != null) {
                CargarImagenBase64(barcodeImage, ImagenCodigoDeBarras.value); // Cargar imagen del código de barras

                // 📝 Activar descarga
                btnDescargar.style.display = "block";
                btnDescargar.onclick = () => DescargarImagen(validarCodigo.value);
            }
        };

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

        btnDescargar.addEventListener("click", (event) => {
            event.preventDefault(); // ❌ Evita que el formulario se envíe
        });
    } else if (currentUrl.includes('/InventarioSalidaProductos/Create')) {

        window.onload = function () {
            if (ImagenProducto != null) {
                CargarImagenBase64(previewImagenProducto, ImagenProducto.value); // Cargar imagen del producto
            }

        };

        validarCodigo.addEventListener("input", () => {
            //Limpiar basura de los campos
            validarCodigo.value = validarCodigo.value.replace(/[^a-zA-Z0-9\-\/\*]/g, '');
            validarCodigo.value = validarCodigo.value.toUpperCase();
            limpiarCampos = true;
            CargarProductoExisteParaVenta(limpiarCampos);
        });

        btnCapturar.addEventListener("click", (event) => {
            event.preventDefault();
            IniciarEscaneo();
            CancelarEscaneo.style.display = 'block';
            btnCapturar.style.display = 'none';
        });

        CancelarEscaneo.addEventListener("click", (event) => {
            event.preventDefault();
            btnCapturar.style.display = 'block';
            CancelarEscaneo.style.display = 'none';
            DetenerEscaneo();
        });                
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
            const response = await fetch(`/Funciones/CargarProductoExisteVenta?filtroId=${codigo}`);

            if (response.ok) {
                const producto = await response.json();

                if (producto && Object.keys(producto).length > 0) { // Verifica si el objeto producto no está vacío
                    productoEncontrado = true;

                    // 2. Llenar los campos con los datos del producto (solo si se encontró)                   
                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
                    cargarUnidadesStock.value = producto.cantStock;

                    lblCargarNombreProducto.setAttribute("data-valor-interno", producto.nombreProducto);
                    lblCargarNombreProducto.innerText = producto.nombreProducto.toUpperCase();
                    cargarNombreProducto.value = producto.nombreProducto.toUpperCase();

                    cargarNombreProveedor.value = producto.nombreProveedor || '';
                    cargarNombreMarca.value = producto.nombreMarca || '';
                    cargarNombreCategoria.value = producto.nombreCategoria || '';
                    cargarNombreSubCategoria.value = producto.nombreSubCategoria || '';
                    cargarColor.value = producto.nombreColor || '';
                    cargarMedida.value = producto.nombreMedida || '';
                    cargarUnidadMedida.value = producto.nombreUnidadMedida || '';
                    cargarTaller.value = producto.taller || '';
                    cargarTallerId.value = producto.tallerId || '';
                    inputPrecioVentaPorUni.value = producto.precioVentaXuni;

                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');

                    CargarImagenBase64(previewImagenProducto, producto.imagenProducto);
                    ImagenProducto.value = null;
                    ImagenProducto.value = producto.imagenProducto;
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
        }

        if (productoEncontrado && ImagenProducto.value != null && ImagenProducto.value != '') {
            previewImagenProducto.style.display = 'block';
            iconoPreviewImagenProducto.style.display = 'none';
        } else if (productoEncontrado == false) {
            previewImagenProducto.style.display = 'none';
            iconoPreviewImagenProducto.style.display = 'block';
        }
    }


    async function CargarProductoExisteParaVenta(limpiarCampos) {
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
            const response = await fetch(`/Funciones/CargarProductoExisteParaVenta?filtroId=${codigo}`);

            if (response.ok) {
                const producto = await response.json();

                // Asegúrate de que el producto se encontró y no es un objeto vacío/nulo
                // Tu API debería devolver un 404 o un objeto vacío si no hay producto.
                // Si devuelve un 200 OK con un objeto vacío o null, maneja eso aquí.
                if (producto && Object.keys(producto).length > 0) { // Verifica si el objeto producto no está vacío
                    productoEncontrado = true;

                    // 2. Llenar los campos con los datos del producto (solo si se encontró)
                    lblCantStockActual.setAttribute("data-valor-interno", producto.cantStock);
                    lblCantStockActual.innerText = `${producto.cantStock} unidades en Stock`;
                    cargarUnidadesStock.value = producto.cantStock;

                    lblCargarNombreProducto.setAttribute("data-valor-interno", producto.nombreProducto);
                    lblCargarNombreProducto.innerText = producto.nombreProducto.toUpperCase();
                    cargarNombreProducto.value = producto.nombreProducto.toUpperCase();

                    cargarNombreProveedor.value = producto.nombreProveedor || '';
                    cargarNombreMarca.value = producto.nombreMarca || '';
                    cargarNombreCategoria.value = producto.nombreCategoria || '';
                    cargarNombreSubCategoria.value = producto.nombreSubCategoria || '';
                    cargarColor.value = producto.nombreColor || '';
                    cargarMedida.value = producto.nombreMedida || '';
                    cargarUnidadMedida.value = producto.nombreUnidadMedida || '';
                    cargarTaller.value = producto.taller || '';
                    cargarTallerId.value = producto.tallerId || '';
                    inputPrecioVentaPorUni.value = producto.precioVentaXuni;

                    formatoMoneda(inputPrecioVentaPorUni, 'resPrecioVentaPorUni');

                    CargarImagenBase64(previewImagenProducto, producto.imagenProducto);
                    ImagenProducto.value = null;
                    ImagenProducto.value = producto.imagenProducto;
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
        }

        if (productoEncontrado && ImagenProducto.value != null && ImagenProducto.value != '') {
            previewImagenProducto.style.display = 'block';
            iconoPreviewImagenProducto.style.display = 'none';
        } else if (productoEncontrado == false) {
            previewImagenProducto.style.display = 'none';
            iconoPreviewImagenProducto.style.display = 'block';
        }        
    }

    function formatoMoneda(input, resultPrecio) {
        const resultadoElement = document.getElementById(resultPrecio);

        // Obtener el valor actual del input y remover los caracteres no numéricos
        let precio = input.value.replace(/\D/g, '');
        //let amount = precio.replace(',', '.');
        let amount = parseInt(precio);

        if (!isNaN(amount)) {
            // Formatear el valor como moneda COP
            const formatoMoneda = new Intl.NumberFormat("es-CO", {
                style: "currency",
                currency: "COP",
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            });
            const cantidadFormateada = formatoMoneda.format(amount);
            input.value = cantidadFormateada;
        } else {
            input.value = '';
            resultadoElement.textContent = "Por favor ingrese un valor válido.";
        }
    }

    // 🎯 Función para descargar la imagen
    async function DescargarImagen(nombreArchivo) {
        const link = document.createElement("a");
        link.href = barcodeCanvas.toDataURL("image/png");
        link.download = `CodigoDeBarras_${nombreArchivo}.png`;
        link.click();
    }

    // 🚀 Función para activar la cámara solo cuando se necesite
    async function IniciarEscaneo() {
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
                DetenerEscaneo();
                CargarProductoExiste(limpiarCampos);
            } else {
                return
            }
        } catch (error) {
            DetenerEscaneo();
        }
    }


    function CargarImagenBase64(canvaImagen, imagenBase) {
        if (canvaImagen.id == 'previewImagenProducto' && (imagenBase == null || imagenBase == '')) {
            iconoPreviewImagenProducto.style.display = 'block';
            previewImagenProducto.style.display = 'none';
        } else if (canvaImagen.id == 'previewImagenProducto' && imagenBase != null) {
            iconoPreviewImagenProducto.style.display = 'none';
            previewImagenProducto.style.display = 'block';
        }

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

    // Detener escaneo y limpiar
    function DetenerEscaneo() {
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
            codeReader = null;
        } catch (error) {
            console.error('Error al detener el escaneo:', error);
        }
    }

    function LimpiarCampos() {
        lblCargarNombreProducto.setAttribute("data-valor-interno", '');
        lblCantStockActual.setAttribute("data-valor-interno", '');
        cargarNombreProveedor.value = '';
        cargarNombreMarca.value = '';
        cargarNombreCategoria.value = '';
        cargarNombreSubCategoria.value = '';
        cargarColor.value = '';
        cargarMedida.value = '';
        cargarUnidadMedida.value = '';
        cargarTaller.value = '';
        cargarTallerId.value = '';
        inputPrecioVentaPorUni.value = '';
        //inputPrecioVentaTotal.value = '';
        //inputPrecioVentaTotalPagado.value = '';
        cargarNombreProducto.value = '';
        cargarUnidadesStock.value = '';
        cantUnidadesVender.value = '';

        previewImagenProducto.style.display = 'none';
        previewImagenProducto.value = null;
        ImagenProducto.value = null;
    }

    // Función para limpiar formato moneda y obtener número
    function parseCurrency(value) {
        if (!value) return 0;
        let cleanValue = value.toString().replace(/[^\d,]/g, '');

        cleanValue = cleanValue.replace(',', '.');

        // 3. Convertimos a número
        return parseFloat(cleanValue) || 0;
    }
       

});