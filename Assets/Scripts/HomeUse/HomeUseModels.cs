using System.Collections.Generic;
using System;

/// <summary>
/// Mapping between SessionExecution in database entities and SessionExecution in ASP.NET MVC model.
/// </summary>
public class PrescribedExercise
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session exercise.
    /// </summary>
    public int Id { get; set; }
    public PrescribedExerciseActivity Activity { get; set; }
    public string ActivityParameters { get; set; }
    public string BodySide { get; set; }
    public int IterationCount { get; set; }
    public string IterationCountText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the visual augmentation support for the session exercise is on or off.
    /// </summary>
    public bool VisualAugmentation { get; set; }
    public int OrderId { get; set; }

    #endregion
}

public class Prescription
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session.
    /// </summary>
    //public int Id { get; set; }

    public string PatientName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public List<PrescribedExercise> PrescribedExercises { get; set; } = new();

    #endregion
}

public class PrescribedExerciseActivity
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session exercise.
    /// </summary>
    public int Id { get; set; }
    public string Name { get; set; }
    public string StartSceneName { get; set; }

    #endregion
}

/// <summary>
/// Mapping between Session in database entities and Session in ASP.NET MVC model.
/// </summary>
public class HomeSession
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session.
    /// </summary>
    public int Id { get; set; }

    #endregion
}

/// <summary>
/// Mapping between SessionExecution in database entities and SessionExecution in ASP.NET MVC model.
/// </summary>
public class CreateHomeSessionExecutionRequest
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session that owns the session exercise.
    /// </summary>
    public int HomeSessionId { get; set; }

    public int PrescribedExerciseId { get; set; }
    public string ActivityResult { get; set; }

    /// <summary>
    /// Gets or sets the duration of the exercise.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    public DateTime FinishDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the visual augmentation support for the session exercise is on or off.
    /// </summary>
    public bool VisualAugmentation { get; set; }

    //public ActivityState ActivityState { get; set; }

    #endregion
}

/// <summary>
/// Mapping between SessionExecution in database entities and SessionExecution in ASP.NET MVC model.
/// </summary>
public class HomeSessionExecution
{
    #region Properties

    /// <summary>
    /// Gets or sets the identifier of the session exercise.
    /// </summary>
    public int Id { get; set; }

    ///// <summary>
    ///// Gets or sets the duration of the exercise.
    ///// </summary>
    //[Required]
    //public int Duration { get; set; }

    ///// <summary>
    ///// Gets or sets the identifier of the exercise state.
    ///// </summary>
    //[Required]
    //public int ActivityState { get; set; }

    //[Required]
    //public Session Session { get; set; }

    //[Required]
    //public Activity Activity { get; set; }
    //public string ActivityParameters { get; set; }
    //public string ActivityResult { get; set; }

    ///// <summary>
    ///// Gets or sets a value indicating whether the visual augmentation support for the session exercise is on or off.
    ///// </summary>
    //[Required]
    //public bool VisualAugmentation { get; set; }

    #endregion
}

