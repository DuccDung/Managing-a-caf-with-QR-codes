using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelCheckQuyen
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelCheckQuyen(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        public int? CheckQuyen(int NvId)
        {
            var result = (from nv in _context.NhanViens
                          join nvpq in _context.NvPqs on nv.NvId equals nvpq.NvId
                          join pq in _context.NvPqs on nvpq.PqId equals pq.PqId
                          where nv.NvId == NvId
                          select pq.PqId).FirstOrDefault();

            return result; // Trả về PqId duy nhất hoặc 0 nếu không có kết quả
        }

    }
}
