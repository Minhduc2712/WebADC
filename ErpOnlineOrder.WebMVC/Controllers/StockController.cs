using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    // [RequirePermission(PermissionCodes.StockView)]
    public class StockController : BaseController
    {
        private readonly IStockApiClient _stockApiClient;
        private readonly IWarehouseApiClient _warehouseApiClient;
        private readonly IProductApiClient _productApiClient;
        private readonly ILogger<StockController> _logger;

        public StockController(
            IStockApiClient stockApiClient,
            IWarehouseApiClient warehouseApiClient,
            IProductApiClient productApiClient,
            ILogger<StockController> logger)
        {
            _stockApiClient = stockApiClient;
            _warehouseApiClient = warehouseApiClient;
            _productApiClient = productApiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? warehouseId, string? searchTerm)
        {
            try
            {
                var stocks = await _stockApiClient.GetAllAsync(warehouseId, searchTerm);
                var warehouses = await _warehouseApiClient.GetForSelectAsync();

                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name", warehouseId);
                ViewBag.SelectedWarehouseId = warehouseId;
                ViewBag.SearchTerm = searchTerm;

                return View(stocks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stocks");
                SetErrorFromException(ex);
                return View(Enumerable.Empty<StockDto>());
            }
        }

        // [RequirePermission(PermissionCodes.StockUpdate)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var warehouses = await _warehouseApiClient.GetForSelectAsync();
                var products = await _productApiClient.GetForOrderAsync();

                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name");
                ViewBag.Products = new SelectList(products.OrderBy(p => p.Product_name), "Id", "Product_name");

                return View(new CreateStockDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock create form");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.StockUpdate)]
        public async Task<IActionResult> Create(CreateStockDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var warehouses = await _warehouseApiClient.GetForSelectAsync();
                    var products = await _productApiClient.GetForOrderAsync();
                    ViewBag.Warehouses = new SelectList(warehouses, "Id", "Warehouse_name", model.Warehouse_id);
                    ViewBag.Products = new SelectList(products.OrderBy(p => p.Product_name), "Id", "Product_name", model.Product_id);
                    return View(model);
                }

                var (created, error) = await _stockApiClient.CreateAsync(model);
                if (created != null)
                {
                    SetSuccessMessage("Thêm tồn kho thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error ?? "Không thể thêm tồn kho. Vui lòng kiểm tra kho, sản phẩm và số lượng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock");
                ModelState.AddModelError(string.Empty, GetDetailedErrorMessage(ex));
            }

            var warehouseList = await _warehouseApiClient.GetForSelectAsync();
            var productList = await _productApiClient.GetForOrderAsync();
            ViewBag.Warehouses = new SelectList(warehouseList, "Id", "Warehouse_name", model.Warehouse_id);
            ViewBag.Products = new SelectList(productList.OrderBy(p => p.Product_name), "Id", "Product_name", model.Product_id);
            return View(model);
        }

        // [RequirePermission(PermissionCodes.StockUpdate)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var stock = await _stockApiClient.GetByIdAsync(id);
                if (stock == null)
                {
                    SetErrorMessage("Không tìm thấy tồn kho.");
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.WarehouseName = stock.Warehouse_name;
                ViewBag.ProductName = stock.Product_name;

                return View(new UpdateStockDto
                {
                    Id = stock.Id,
                    Quantity = stock.Quantity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stock for edit");
                SetErrorFromException(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RequirePermission(PermissionCodes.StockUpdate)]
        public async Task<IActionResult> Edit(int id, UpdateStockDto model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    var stock = await _stockApiClient.GetByIdAsync(model.Id);
                    ViewBag.WarehouseName = stock?.Warehouse_name ?? string.Empty;
                    ViewBag.ProductName = stock?.Product_name ?? string.Empty;
                    return View(model);
                }

                var (success, error) = await _stockApiClient.UpdateAsync(id, model);
                if (success)
                {
                    SetSuccessMessage("Cập nhật tồn kho thành công!");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error ?? "Cập nhật tồn kho thất bại. Vui lòng kiểm tra số lượng tồn và thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                ModelState.AddModelError(string.Empty, GetDetailedErrorMessage(ex));
            }

            var current = await _stockApiClient.GetByIdAsync(model.Id);
            ViewBag.WarehouseName = current?.Warehouse_name ?? string.Empty;
            ViewBag.ProductName = current?.Product_name ?? string.Empty;
            return View(model);
        }
    }
}
