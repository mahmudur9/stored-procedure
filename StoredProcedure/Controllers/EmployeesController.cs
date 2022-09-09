using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StoredProcedure.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StoredProcedure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> EmployeeList()
        {
            DataTable employees = new DataTable();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                SqlDataAdapter adapter = new SqlDataAdapter("StoredProcedure", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("Event", "SELECT");
                await Task.Run(() =>
                {
                    adapter.Fill(employees);
                });
                await connection.CloseAsync();
            }

            return Ok(employees);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            DataTable employees = new DataTable();
            Employee employee = new Employee();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                SqlDataAdapter adapter = new SqlDataAdapter("StoredProcedure", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("Event", "SELECTONE");
                adapter.SelectCommand.Parameters.AddWithValue("EmployeeId", id);
                await Task.Run(() =>
                {
                    adapter.Fill(employees);
                });
                await connection.CloseAsync();
            }

            if (employees.Rows.Count == 1)
            {
                employee.EmployeeId = int.Parse(employees.Rows[0]["EmployeeId"].ToString());
                employee.Name = employees.Rows[0]["Name"].ToString();
                employee.Phone = employees.Rows[0]["Phone"].ToString();
                employee.Salary = decimal.Parse(employees.Rows[0]["Salary"].ToString());
            }
            else
            {
                return NoContent();
            }

            return Ok(employee);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee(Employee employee)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand("StoredProcedure", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("Event", "ADD");
                command.Parameters.AddWithValue("Name", employee.Name);
                command.Parameters.AddWithValue("Phone", employee.Phone);
                command.Parameters.AddWithValue("Salary", employee.Salary);
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

            return Ok("Employee added successfully!");
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEmployee(Employee employee, int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand("StoredProcedure", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("Event", "UPDATE");
                command.Parameters.AddWithValue("Name", employee.Name);
                command.Parameters.AddWithValue("Phone", employee.Phone);
                command.Parameters.AddWithValue("Salary", employee.Salary);
                command.Parameters.AddWithValue("EmployeeId", id);
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

            return Ok("Employee updated successfully!");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand("StoredProcedure", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("Event", "DELETE");
                command.Parameters.AddWithValue("EmployeeId", id);
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

            return Ok("Employee deleted successfully!");
        }
    }
}
