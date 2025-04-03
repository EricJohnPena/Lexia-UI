// Define the Lesson model
class Lesson {
    constructor(public id: string, public title: string) {}
}

// Define the Module model
class Module {
    constructor(public id: string, public title: string, public lessons: Lesson[] = []) {}
}

// Define the Subject model
class Subject {
    constructor(public id: string, public name: string, public modules: Module[] = []) {}
}

// SubjectTracker class to manage subjects, modules, and lessons
export class SubjectTracker {
    private subjects: Subject[] = [];

    // Add a new subject
    addSubject(id: string, name: string): void {
        this.subjects.push(new Subject(id, name));
    }

    // Add a module to a subject
    addModule(subjectId: string, moduleId: string, title: string): void {
        const subject = this.subjects.find(s => s.id === subjectId);
        if (subject) {
            subject.modules.push(new Module(moduleId, title));
        }
    }

    // Add a lesson to a module
    addLesson(subjectId: string, moduleId: string, lessonId: string, title: string): void {
        const subject = this.subjects.find(s => s.id === subjectId);
        const module = subject?.modules.find(m => m.id === moduleId);
        if (module) {
            module.lessons.push(new Lesson(lessonId, title));
        }
    }

    // Retrieve all subjects
    getSubjects(): Subject[] {
        return this.subjects;
    }

    // Retrieve a specific subject by ID
    getSubjectById(subjectId: string): Subject | undefined {
        return this.subjects.find(s => s.id === subjectId);
    }
}
