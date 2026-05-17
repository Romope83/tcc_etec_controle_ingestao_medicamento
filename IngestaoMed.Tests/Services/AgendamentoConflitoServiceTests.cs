using Moq;
using Xunit;
using IngestaoMed.Core.Services;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Enums;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;


//Modificado em 16.05.2026
namespace IngestaoMed.Tests.Services
{
    public class AgendamentoConflitoServiceTests
    {
        private readonly Mock<IDatabaseContext> _dbMock;
        private readonly AgendamentoConflitoService _service;

        public AgendamentoConflitoServiceTests()
        {
            _dbMock = new Mock<IDatabaseContext>();
            _service = new AgendamentoConflitoService(_dbMock.Object);
        }

        #region Testes: VerificarConflitosAsync

        [Fact]
        public async Task VerificarConflitosAsync_DadosEntradaInvalidos_DeveRetornarVazio()
        {
            // Act
            var res1 = await _service.VerificarConflitosAsync(null!, new List<DateTime> { DateTime.Now });
            var res2 = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = 1 }, null!);
            var res3 = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = 1 }, new List<DateTime>());

            // Assert
            Assert.Empty(res1);
            Assert.Empty(res2);
            Assert.Empty(res3);
        }

        [Fact]
        public async Task VerificarConflitosAsync_PacienteSemTratamentos_DeveRetornarVazio()
        {
            // Arrange
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento>());

            // Act
            var resultado = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = 1 }, new List<DateTime> { DateTime.Now });

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task VerificarConflitosAsync_HouverConflitoMenorQue30Minutos_DeveRetornarAgendamentos()
        {
            // Arrange
            int pacienteId = 1;
            int tratamentoId = 10;
            int vinculoId = 100;
            var horaBase = new DateTime(2026, 5, 16, 12, 0, 0);

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamentoId, PacienteId = pacienteId } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento> { new MedicamentoTratamento { Id = vinculoId, TratamentoId = tratamentoId } });

            var agendamentosExistentes = new List<Agendamento>
            {
                new Agendamento { Id = 1, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase, Status = "Pendente" },
                new Agendamento { Id = 2, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase.AddMinutes(29), Status = "Pendente" }
            };

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamentosExistentes);

            var novosHorarios = new List<DateTime> { horaBase };

            // Act
            var resultado = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = pacienteId }, novosHorarios);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, a => a.Id == 1);
            Assert.Contains(resultado, a => a.Id == 2);
        }

        [Fact]
        public async Task VerificarConflitosAsync_HorariosMaioresOuIguaisA30Minutos_DeveRetornarVazio()
        {
            // Arrange
            int pacienteId = 1;
            int tratamientoId = 10;
            int vinculoId = 100;
            var horaBase = new DateTime(2026, 5, 16, 12, 0, 0);

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamientoId, PacienteId = pacienteId } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento> { new MedicamentoTratamento { Id = vinculoId, TratamentoId = tratamientoId } });

            var agendamentosExistentes = new List<Agendamento>
            {
                new Agendamento { Id = 1, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase.AddMinutes(30), Status = "Pendente" },
                new Agendamento { Id = 2, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase.AddHours(2), Status = "Pendente" }
            };

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamentosExistentes);

            var novosHorarios = new List<DateTime> { horaBase };

            // Act
            var resultado = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = pacienteId }, novosHorarios);

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task VerificarConflitosAsync_AgendamentosNaoPendentes_DeveIgnorarERetornarVazio()
        {
            // Arrange
            int pacienteId = 1;
            int tratamentoId = 10;
            int vinculoId = 100;
            var horaBase = new DateTime(2026, 5, 16, 12, 0, 0);

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamentoId, PacienteId = pacienteId } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento> { new MedicamentoTratamento { Id = vinculoId, TratamentoId = tratamentoId } });

            // Modificado: Definimos horários intencionalmente distantes (ex: 5 dias depois) para que o filtro de 30 minutos os ignore de qualquer forma
            var agendamentosExistentes = new List<Agendamento>
    {
        new Agendamento { Id = 1, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase.AddDays(5), Status = "Tomado" },
        new Agendamento { Id = 2, MedicamentoTratamentoId = vinculoId, ProximoAlarme = horaBase.AddDays(5), Status = "Cancelado" }
    };

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(agendamentosExistentes);

            // Act
            var resultado = await _service.VerificarConflitosAsync(new Tratamento { PacienteId = pacienteId }, new List<DateTime> { horaBase });

            // Assert
            Assert.Empty(resultado);
        }

        #endregion

        #region Testes: VerificarDuplicidadeMedicamentoEmAndamentoAsync

        [Fact]
        public async Task VerificarDuplicidadeMedicamentoEmAndamentoAsync_PacienteSemTratamentos_DeveRetornarFalse()
        {
            // Arrange
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento>());

            // Act
            var resultado = await _service.VerificarDuplicidadeMedicamentoEmAndamentoAsync(1, 5, 0);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task VerificarDuplicidadeMedicamentoEmAndamentoAsync_SemVinculosAtivosDoMesmoRemedio_DeveRetornarFalse()
        {
            // Arrange
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = 10, PacienteId = 1 } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento>());

            // Act
            var resultado = await _service.VerificarDuplicidadeMedicamentoEmAndamentoAsync(1, 5, 0);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task VerificarDuplicidadeMedicamentoEmAndamentoAsync_MesmoRemedioAtivoComDosePendente_DeveRetornarTrue()
        {
            // Arrange
            int pacienteId = 1;
            int medicamentoId = 5;
            int tratamentoId = 10;
            int vinculoExistenteId = 100;
            int vinculoAtualId = 0;

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamentoId, PacienteId = pacienteId } });

            var vinculosBancarios = new List<MedicamentoTratamento>
            {
                new MedicamentoTratamento { Id = vinculoExistenteId, TratamentoId = tratamentoId, MedicamentoId = medicamentoId, Ativo = true }
            };
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(vinculosBancarios);

            var dosesBancarias = new List<Agendamento>
            {
                new Agendamento { Id = 50, MedicamentoTratamentoId = vinculoExistenteId, Status = "Pendente" }
            };
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(dosesBancarias);

            // Act
            var resultado = await _service.VerificarDuplicidadeMedicamentoEmAndamentoAsync(pacienteId, medicamentoId, vinculoAtualId);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public async Task VerificarDuplicidadeMedicamentoEmAndamentoAsync_MesmoRemedioSemDosesPendentes_DeveRetornarFalse()
        {
            // Arrange
            int pacienteId = 1;
            int medicamentoId = 5;
            int tratamentoId = 10;
            int vinculoExistenteId = 100;

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamentoId, PacienteId = pacienteId } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento> {
               new MedicamentoTratamento { Id = vinculoExistenteId, TratamentoId = tratamentoId, MedicamentoId = medicamentoId, Ativo = true }
                   });
            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Agendamento, bool>>>()))
                   .ReturnsAsync(new List<Agendamento>());

            // Act
            var resultado = await _service.VerificarDuplicidadeMedicamentoEmAndamentoAsync(pacienteId, medicamentoId, 0);

            // Assert
            Assert.False(resultado);
        }
        [Fact]
        public async Task VerificarDuplicidadeMedicamentoEmAndamentoAsync_ModoEdicaoDesconsiderarOProprioRegistro_DeveRetornarFalse()
        {
            // Arrange
            int pacienteId = 1;
            int medicamentoId = 5;
            int tratamentoId = 10;
            int vinculoAtualId = 100;

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<Tratamento, bool>>>()))
                   .ReturnsAsync(new List<Tratamento> { new Tratamento { Id = tratamentoId, PacienteId = pacienteId } });

            _dbMock.Setup(x => x.BuscarOndeAsync(It.IsAny<Expression<Func<MedicamentoTratamento, bool>>>()))
                   .ReturnsAsync(new List<MedicamentoTratamento>());

            // Act
            var resultado = await _service.VerificarDuplicidadeMedicamentoEmAndamentoAsync(pacienteId, medicamentoId, vinculoAtualId);

            // Assert
            Assert.False(resultado);
        }

        #endregion
    }
}