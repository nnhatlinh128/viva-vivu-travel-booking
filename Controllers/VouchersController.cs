using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Common;
using Microsoft.EntityFrameworkCore;

namespace ToursAndTravelsManagement.Controllers
{
    public class VouchersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VouchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Vouchers
                .OrderByDescending(v => v.VoucherId);

            var pagedList = await PaginatedList<Voucher>
                .CreateAsync(query, pageNumber, pageSize);

            ViewBag.PageSize = pageSize;
            return View(pagedList);
        }

        // ================== CREATE ==================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (!ModelState.IsValid)
                return View(voucher);

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm voucher thành công";
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voucher voucher)
        {
            if (id != voucher.VoucherId) return NotFound();
            if (!ModelState.IsValid) return View(voucher);

            _context.Update(voucher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật voucher thành công";
            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE ==================
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa voucher";
            return RedirectToAction(nameof(Index));
        }
                
// ================== ApplyVoucher ==================
        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string code, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Mã voucher không hợp lệ" });

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == code && v.IsActive);

            if (voucher == null)
                return Json(new { success = false, message = "Voucher không tồn tại" });

            var now = DateTime.UtcNow;

            if (now < voucher.StartDate || now > voucher.EndDate)
                return Json(new { success = false, message = "Voucher đã hết hạn" });

            if (voucher.Quantity <= 0)
                return Json(new { success = false, message = "Voucher đã hết lượt sử dụng" });

            decimal discountAmount;

            if (voucher.IsPercentage)
                discountAmount = totalAmount * voucher.DiscountValue / 100;
            else
                discountAmount = voucher.DiscountValue;

            if (discountAmount > totalAmount)
                discountAmount = totalAmount;

            var finalPrice = totalAmount - discountAmount;

            return Json(new
            {
                success = true,
                voucherId = voucher.VoucherId,
                discountAmount,
                finalPrice = totalAmount - discountAmount
            });
        }
    }
}

